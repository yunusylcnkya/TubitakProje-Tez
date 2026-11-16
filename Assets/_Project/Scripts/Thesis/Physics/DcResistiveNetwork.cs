
using System;
using System.Collections.Generic;

namespace Thesis.Physics
{
    public sealed class DcNetworkResult
    {
        public DcSupplyResult Supply { get; }
        public double EquivalentResistanceOhms { get; }
        public bool HasFloatingNodes { get; }

        public IReadOnlyDictionary<string, double>
            NodeVoltages
        { get; }

        public IReadOnlyDictionary<string, double>
            BranchCurrents
        { get; }

        public DcNetworkResult(
            DcSupplyResult supply,
            double equivalentResistance,
            bool hasFloatingNodes,
            Dictionary<string, double> voltages,
            Dictionary<string, double> currents)
        {
            Supply = supply;
            EquivalentResistanceOhms =
                equivalentResistance;
            HasFloatingNodes = hasFloatingNodes;
            NodeVoltages = voltages;
            BranchCurrents = currents;
        }
    }

    public sealed class DcResistiveNetwork
    {
        private sealed class Edge
        {
            public string Id;
            public string A;
            public string B;
            public double Resistance;
        }

        private readonly List<Edge> wires =
            new List<Edge>();

        private readonly List<Edge> resistors =
            new List<Edge>();

        public void AddWire(string a, string b)
        {
            ValidateName(a);
            ValidateName(b);

            wires.Add(new Edge { A = a, B = b });
        }

        public void AddResistor(
            string id,
            string a,
            string b,
            double resistanceOhms)
        {
            ValidateName(id);
            ValidateName(a);
            ValidateName(b);

            if (double.IsNaN(resistanceOhms) ||
                double.IsInfinity(resistanceOhms) ||
                resistanceOhms <= 0 ||
                double.IsInfinity(1.0 / resistanceOhms))
                throw new ArgumentOutOfRangeException(
                    nameof(resistanceOhms));

            foreach (var resistor in resistors)
                if (resistor.Id == id)
                    throw new ArgumentException(
                        "Duplicate resistor ID.");

            resistors.Add(new Edge
            {
                Id = id,
                A = a,
                B = b,
                Resistance = resistanceOhms
            });
        }

        public DcNetworkResult Solve(
            string positive,
            string negative,
            RegulatedDcSupplyModel supply,
            bool acInputValid,
            bool switchEnabled)
        {
            ValidateName(positive);
            ValidateName(negative);

            if (supply == null)
                throw new ArgumentNullException(
                    nameof(supply));

            var parent = new Dictionary<string, string>(
                StringComparer.Ordinal);

            Register(parent, positive);
            Register(parent, negative);

            foreach (var wire in wires)
            {
                Register(parent, wire.A);
                Register(parent, wire.B);
                Union(parent, wire.A, wire.B);
            }

            foreach (var resistor in resistors)
            {
                Register(parent, resistor.A);
                Register(parent, resistor.B);
            }

            var roots = new HashSet<string>(
                StringComparer.Ordinal);

            var terminalNames =
                new List<string>(parent.Keys);

            foreach (string name in terminalNames)
                roots.Add(Find(parent, name));

            var sortedRoots = new List<string>(roots);
            sortedRoots.Sort(StringComparer.Ordinal);

            var index = new Dictionary<string, int>(
                StringComparer.Ordinal);

            for (int i = 0; i < sortedRoots.Count; i++)
                index[sortedRoots[i]] = i;

            int positiveNet =
                index[Find(parent, positive)];

            int negativeNet =
                index[Find(parent, negative)];

            int count = sortedRoots.Count;

            var branchA = new int[resistors.Count];
            var branchB = new int[resistors.Count];

            for (int i = 0; i < resistors.Count; i++)
            {
                branchA[i] =
                    index[Find(parent, resistors[i].A)];

                branchB[i] =
                    index[Find(parent, resistors[i].B)];
            }

            // Direct wire between source terminals.
            if (positiveNet == negativeNet)
            {
                var shortResult = supply.Solve(
                    0, acInputValid, switchEnabled);

                var zeroVoltages =
                    new Dictionary<string, double>();

                foreach (string name in terminalNames)
                    zeroVoltages[name] = 0;

                var zeroCurrents =
                    new Dictionary<string, double>();

                foreach (var resistor in resistors)
                    zeroCurrents[resistor.Id] = 0;

                return new DcNetworkResult(
                    shortResult, 0, false,
                    zeroVoltages, zeroCurrents);
            }

            // Resistor adjacency.
            var adjacent = new List<int>[count];

            for (int i = 0; i < count; i++)
                adjacent[i] = new List<int>();

            for (int i = 0; i < resistors.Count; i++)
            {
                int a = branchA[i];
                int b = branchB[i];

                if (a == b) continue;

                adjacent[a].Add(b);
                adjacent[b].Add(a);
            }

            // Find networks connected to either source pole.
            var reachable = new bool[count];
            var queue = new Queue<int>();

            reachable[positiveNet] = true;
            reachable[negativeNet] = true;

            queue.Enqueue(positiveNet);
            queue.Enqueue(negativeNet);

            while (queue.Count > 0)
            {
                int node = queue.Dequeue();

                foreach (int next in adjacent[node])
                {
                    if (reachable[next]) continue;

                    reachable[next] = true;
                    queue.Enqueue(next);
                }
            }

            bool hasFloatingNodes = false;

            var unknownRows =
                new Dictionary<int, int>();

            for (int node = 0; node < count; node++)
            {
                if (!reachable[node])
                {
                    hasFloatingNodes = true;
                    continue;
                }

                if (node == positiveNet ||
                    node == negativeNet)
                    continue;

                unknownRows[node] = unknownRows.Count;
            }

            int n = unknownRows.Count;

            var matrix = new double[n, n];
            var rhs = new double[n];

            for (int i = 0; i < resistors.Count; i++)
            {
                int a = branchA[i];
                int b = branchB[i];

                if (a == b || !reachable[a])
                    continue;

                double conductance =
                    1.0 / resistors[i].Resistance;

                AddEquation(
                    a, b, conductance,
                    positiveNet, unknownRows,
                    matrix, rhs);

                AddEquation(
                    b, a, conductance,
                    positiveNet, unknownRows,
                    matrix, rhs);
            }

            double[] solved = SolveLinear(matrix, rhs);

            var unitVoltages = new double[count];

            for (int i = 0; i < count; i++)
                unitVoltages[i] = double.NaN;

            unitVoltages[positiveNet] = 1;
            unitVoltages[negativeNet] = 0;

            foreach (var item in unknownRows)
                unitVoltages[item.Key] =
                    solved[item.Value];

            // Source current for 1 volt applied.
            double unitCurrent = 0;

            for (int i = 0; i < resistors.Count; i++)
            {
                int a = branchA[i];
                int b = branchB[i];

                if (a == b || !reachable[a])
                    continue;

                double current =
                    (unitVoltages[a] - unitVoltages[b]) /
                    resistors[i].Resistance;

                if (a == positiveNet)
                    unitCurrent += current;

                if (b == positiveNet)
                    unitCurrent -= current;
            }

            double equivalentResistance =
                unitCurrent > 1e-15
                    ? 1.0 / unitCurrent
                    : double.PositiveInfinity;

            // Existing v0.1 CV/CC supply model.
            DcSupplyResult sourceResult = supply.Solve(
                equivalentResistance,
                acInputValid,
                switchEnabled);

            double outputVoltage =
                sourceResult.VoltageVolts;

            var voltages =
                new Dictionary<string, double>(
                    StringComparer.Ordinal);

            foreach (string name in terminalNames)
            {
                int net = index[Find(parent, name)];

                voltages[name] = reachable[net]
                    ? unitVoltages[net] * outputVoltage
                    : double.NaN;
            }

            var currents =
                new Dictionary<string, double>(
                    StringComparer.Ordinal);

            for (int i = 0; i < resistors.Count; i++)
            {
                int a = branchA[i];
                int b = branchB[i];

                currents[resistors[i].Id] =
                    a == b || !reachable[a]
                        ? 0
                        : (unitVoltages[a] -
                           unitVoltages[b]) *
                          outputVoltage /
                          resistors[i].Resistance;
            }

            return new DcNetworkResult(
                sourceResult,
                equivalentResistance,
                hasFloatingNodes,
                voltages,
                currents);
        }

        private static void AddEquation(
            int node,
            int other,
            double conductance,
            int positiveNet,
            Dictionary<int, int> rows,
            double[,] matrix,
            double[] rhs)
        {
            if (!rows.TryGetValue(node, out int row))
                return;

            matrix[row, row] += conductance;

            if (rows.TryGetValue(other, out int column))
            {
                matrix[row, column] -= conductance;
            }
            else if (other == positiveNet)
            {
                rhs[row] += conductance;
            }
        }

        private static double[] SolveLinear(
            double[,] matrix,
            double[] rhs)
        {
            int n = rhs.Length;

            for (int k = 0; k < n; k++)
            {
                int pivot = k;

                for (int i = k + 1; i < n; i++)
                    if (Math.Abs(matrix[i, k]) >
                        Math.Abs(matrix[pivot, k]))
                        pivot = i;

                if (Math.Abs(matrix[pivot, k]) < 1e-20)
                    throw new InvalidOperationException(
                        "Singular or ill-conditioned network.");

                if (pivot != k)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double temp = matrix[k, j];
                        matrix[k, j] = matrix[pivot, j];
                        matrix[pivot, j] = temp;
                    }

                    double tmp = rhs[k];
                    rhs[k] = rhs[pivot];
                    rhs[pivot] = tmp;
                }

                double diagonal = matrix[k, k];

                for (int j = k; j < n; j++)
                    matrix[k, j] /= diagonal;

                rhs[k] /= diagonal;

                for (int i = 0; i < n; i++)
                {
                    if (i == k) continue;

                    double factor = matrix[i, k];

                    for (int j = k; j < n; j++)
                        matrix[i, j] -=
                            factor * matrix[k, j];

                    rhs[i] -= factor * rhs[k];
                }
            }

            return rhs;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Terminal or element ID is required.");
        }

        private static void Register(
            Dictionary<string, string> parent,
            string name)
        {
            if (!parent.ContainsKey(name))
                parent[name] = name;
        }

        private static string Find(
            Dictionary<string, string> parent,
            string name)
        {
            if (parent[name] != name)
                parent[name] =
                    Find(parent, parent[name]);

            return parent[name];
        }

        private static void Union(
            Dictionary<string, string> parent,
            string a,
            string b)
        {
            string rootA = Find(parent, a);
            string rootB = Find(parent, b);

            if (rootA != rootB)
                parent[rootB] = rootA;
        }
    }
}
