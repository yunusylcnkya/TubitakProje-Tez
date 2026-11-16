
using System;
using NUnit.Framework;
using Thesis.Physics;

public class DcResistiveNetworkTests
{
    private static RegulatedDcSupplyModel Supply()
    {
        return new RegulatedDcSupplyModel(24, 5);
    }

    [Test]
    public void SingleResistor_CalculatesOneAmpere()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "N", 24);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(
            result.Supply.VoltageVolts,
            Is.EqualTo(24).Within(1e-8));

        Assert.That(
            result.BranchCurrents["R1"],
            Is.EqualTo(1).Within(1e-8));
    }

    [Test]
    public void SeriesResistors_CalculateMidpoint()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "M", 12);
        circuit.AddResistor("R2", "M", "N", 12);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.NodeVoltages["M"],
            Is.EqualTo(12).Within(1e-8));

        Assert.That(result.BranchCurrents["R1"],
            Is.EqualTo(1).Within(1e-8));

        Assert.That(result.BranchCurrents["R2"],
            Is.EqualTo(1).Within(1e-8));
    }

    [Test]
    public void ParallelResistors_AddCurrents()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "N", 24);
        circuit.AddResistor("R2", "P", "N", 48);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.Supply.CurrentAmperes,
            Is.EqualTo(1.5).Within(1e-8));

        Assert.That(result.EquivalentResistanceOhms,
            Is.EqualTo(16).Within(1e-8));
    }

    [Test]
    public void ExcessLoad_LimitsCurrent()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "N", 3);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.Supply.Mode,
            Is.EqualTo(DcSupplyMode.CurrentLimited));

        Assert.That(result.Supply.VoltageVolts,
            Is.EqualTo(15).Within(1e-8));

        Assert.That(result.Supply.CurrentAmperes,
            Is.EqualTo(5).Within(1e-8));
    }

    [Test]
    public void OpenCircuit_HasZeroCurrent()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "X", 24);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.Supply.CurrentAmperes,
            Is.EqualTo(0).Within(1e-8));

        Assert.That(result.NodeVoltages["X"],
            Is.EqualTo(24).Within(1e-8));
    }

    [Test]
    public void WireConnection_MergesNodes()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddWire("P", "A");
        circuit.AddResistor("R1", "A", "N", 24);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.NodeVoltages["A"],
            Is.EqualTo(24).Within(1e-8));

        Assert.That(result.BranchCurrents["R1"],
            Is.EqualTo(1).Within(1e-8));
    }

    [Test]
    public void IsolatedNetwork_IsMarkedFloating()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "X", "Y", 10);

        var result = circuit.Solve(
            "P", "N", Supply(), true, true);

        Assert.That(result.HasFloatingNodes, Is.True);
        Assert.That(
            double.IsNaN(result.NodeVoltages["X"]),
            Is.True);
    }

    [Test]
    public void SourceSwitchedOff_HasZeroOutput()
    {
        var circuit = new DcResistiveNetwork();

        circuit.AddResistor("R1", "P", "N", 24);

        var result = circuit.Solve(
            "P", "N", Supply(), true, false);

        Assert.That(result.Supply.Mode,
            Is.EqualTo(DcSupplyMode.Off));

        Assert.That(result.Supply.CurrentAmperes,
            Is.EqualTo(0));
    }
}
