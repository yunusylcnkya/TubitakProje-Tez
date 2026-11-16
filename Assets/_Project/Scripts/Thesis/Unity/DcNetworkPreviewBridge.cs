
using System;
using System.Collections.Generic;
using Thesis.Physics;
using UnityEngine;

public sealed class DcNetworkPreviewBridge : MonoBehaviour
{
    [Serializable]
    private sealed class ResistiveLoad
    {
        public string id = "LAMP1";
        public Terminal terminalA;
        public Terminal terminalB;

        [Min(0.001f)]
        public float resistanceOhms = 1200f;
    }

    [Serializable]
    private sealed class FuseLink
    {
        public FuseDevice fuse;
        public Terminal input;
        public Terminal output;
    }

    private struct TerminalEdge
    {
        public Terminal A;
        public Terminal B;

        public TerminalEdge(Terminal a, Terminal b)
        {
            A = a;
            B = b;
        }
    }

    [Header("Existing Unity Systems")]
    [SerializeField]
    private ConnectionManager connectionManager;

    [SerializeField]
    private ElectricalSimulationSystem legacySimulation;

    [SerializeField]
    private DCPowerSource dcSource;

    [Header("Initial Physical Parameters")]
    [SerializeField, Min(0.01f)]
    private float nominalVoltage = 24f;

    [SerializeField, Min(0.01f)]
    private float currentLimit = 5f;

    [Header("Resistive Load Models")]
    [SerializeField]
    private ResistiveLoad[] loads;

    [Header("Fuse Internal Connections")]
    [SerializeField]
    private FuseLink[] fuseLinks;

    [Header("Diagnostics")]
    [SerializeField]
    private bool logResults = true;

    public DcNetworkResult LastResult { get; private set; }

    private void OnEnable()
    {
        if (connectionManager != null)
            connectionManager.ConnectionsChanged += Recalculate;

        if (legacySimulation != null)
            legacySimulation.SimulationChanged += OnSimulationChanged;

        if (dcSource != null)
            dcSource.StateChanged += Recalculate;
    }

    private void Start()
    {
        Recalculate();
    }

    private void OnDisable()
    {
        if (connectionManager != null)
            connectionManager.ConnectionsChanged -= Recalculate;

        if (legacySimulation != null)
            legacySimulation.SimulationChanged -= OnSimulationChanged;

        if (dcSource != null)
            dcSource.StateChanged -= Recalculate;
    }

    private void OnSimulationChanged(
        ElectricalSimulationSnapshot snapshot)
    {
        Recalculate();
    }

    [ContextMenu("Recalculate DC Network")]
    public void Recalculate()
    {
        LastResult = null;

        if (connectionManager == null ||
            dcSource == null ||
            dcSource.PositiveTerminal == null ||
            dcSource.NegativeTerminal == null)
        {
            return;
        }

        try
        {
            var wireEdges = new List<TerminalEdge>();
            var closedFuseEdges = new List<TerminalEdge>();

            var adjacency =
                new Dictionary<Terminal, List<Terminal>>();

            // Collect actual player-created wires.
            foreach (Connection connection
                     in connectionManager.Connections)
            {
                if (connection == null ||
                    connection.TerminalA == null ||
                    connection.TerminalB == null)
                    continue;

                var edge = new TerminalEdge(
                    connection.TerminalA,
                    connection.TerminalB);

                wireEdges.Add(edge);
                Connect(adjacency, edge.A, edge.B);
            }

            // Add only closed/conducting fuse contacts.
            if (fuseLinks != null)
            {
                foreach (FuseLink link in fuseLinks)
                {
                    if (link == null ||
                        link.fuse == null ||
                        !link.fuse.IsConducting ||
                        link.input == null ||
                        link.output == null)
                        continue;

                    var edge = new TerminalEdge(
                        link.input,
                        link.output);

                    closedFuseEdges.Add(edge);
                    Connect(adjacency, edge.A, edge.B);
                }
            }

            // Register modeled loads for connectivity.
            if (loads != null)
            {
                foreach (ResistiveLoad load in loads)
                {
                    if (!IsValidLoad(load))
                        continue;

                    Connect(
                        adjacency,
                        load.terminalA,
                        load.terminalB);
                }
            }

            // Restrict the solved network to the
            // components connected to this DC source.
            var reachable = new HashSet<Terminal>();
            var queue = new Queue<Terminal>();

            Enqueue(dcSource.PositiveTerminal, reachable, queue);
            Enqueue(dcSource.NegativeTerminal, reachable, queue);

            while (queue.Count > 0)
            {
                Terminal current = queue.Dequeue();

                if (!adjacency.TryGetValue(
                        current,
                        out List<Terminal> neighbours))
                    continue;

                foreach (Terminal next in neighbours)
                    Enqueue(next, reachable, queue);
            }

            var network = new DcResistiveNetwork();

            foreach (TerminalEdge edge in wireEdges)
            {
                if (reachable.Contains(edge.A) &&
                    reachable.Contains(edge.B))
                {
                    network.AddWire(
                        Key(edge.A),
                        Key(edge.B));
                }
            }

            foreach (TerminalEdge edge in closedFuseEdges)
            {
                if (reachable.Contains(edge.A) &&
                    reachable.Contains(edge.B))
                {
                    network.AddWire(
                        Key(edge.A),
                        Key(edge.B));
                }
            }

            if (loads != null)
            {
                foreach (ResistiveLoad load in loads)
                {
                    if (!IsValidLoad(load) ||
                        !reachable.Contains(load.terminalA) ||
                        !reachable.Contains(load.terminalB))
                        continue;

                    network.AddResistor(
                        load.id,
                        Key(load.terminalA),
                        Key(load.terminalB),
                        load.resistanceOhms);
                }
            }

            var supply = new RegulatedDcSupplyModel(
                nominalVoltage,
                currentLimit);

            // Existing simulation decides whether
            // the physical PSU is operational.
            bool acInputValid = dcSource.HasValidACInput;
            bool switchedOn = dcSource.IsActive;

            LastResult = network.Solve(
                Key(dcSource.PositiveTerminal),
                Key(dcSource.NegativeTerminal),
                supply,
                acInputValid,
                switchedOn);

            if (logResults)
                WriteResults();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "THESIS DC solver failed:\n" + exception,
                this);
        }
    }

    private void WriteResults()
    {
        if (LastResult == null)
            return;

        DcSupplyResult output = LastResult.Supply;

        Debug.Log(
            $"THESIS DC | {output.Mode} | " +
            $"Voltage: {output.VoltageVolts:F3} V | " +
            $"Current: {output.CurrentAmperes:F4} A | " +
            $"Power: {output.OutputPowerWatts:F3} W",
            this);

        if (loads == null)
            return;

        foreach (ResistiveLoad load in loads)
        {
            if (load == null || string.IsNullOrWhiteSpace(load.id))
                continue;

            if (LastResult.BranchCurrents.TryGetValue(
                    load.id, out double current))
            {
                Debug.Log(
                    $"THESIS LOAD | {load.id} | " +
                    $"I = {current:F4} A",
                    this);
            }
        }
    }

    private static bool IsValidLoad(ResistiveLoad load)
    {
        if (load == null ||
            load.terminalA == null ||
            load.terminalB == null ||
            load.terminalA == load.terminalB ||
            string.IsNullOrWhiteSpace(load.id) ||
            load.resistanceOhms <= 0)
            return false;

        Equipment owner = load.terminalA.Owner;

        return owner == null ||
               owner.CurrentState == EquipmentState.Placed;
    }

    private static string Key(Terminal terminal)
    {
        return "T_" + terminal.GetEntityId().ToString();
    }

    private static void Enqueue(
        Terminal terminal,
        HashSet<Terminal> visited,
        Queue<Terminal> queue)
    {
        if (terminal != null && visited.Add(terminal))
            queue.Enqueue(terminal);
    }

    private static void Connect(
        Dictionary<Terminal, List<Terminal>> adjacency,
        Terminal a,
        Terminal b)
    {
        AddNeighbour(adjacency, a, b);
        AddNeighbour(adjacency, b, a);
    }

    private static void AddNeighbour(
        Dictionary<Terminal, List<Terminal>> adjacency,
        Terminal a,
        Terminal b)
    {
        if (a == null || b == null)
            return;

        if (!adjacency.TryGetValue(
                a, out List<Terminal> neighbours))
        {
            neighbours = new List<Terminal>();
            adjacency[a] = neighbours;
        }

        if (!neighbours.Contains(b))
            neighbours.Add(b);
    }
}
