
using System;

namespace Thesis.Physics
{
    public enum DcSupplyMode
    {
        Off,
        ConstantVoltage,
        CurrentLimited,
        ShortCircuit
    }

    public readonly struct DcSupplyResult
    {
        public DcSupplyMode Mode { get; }
        public double VoltageVolts { get; }
        public double CurrentAmperes { get; }

        public double OutputPowerWatts =>
            VoltageVolts * CurrentAmperes;

        public DcSupplyResult(
            DcSupplyMode mode,
            double voltage,
            double current)
        {
            Mode = mode;
            VoltageVolts = voltage;
            CurrentAmperes = current;
        }
    }

    public sealed class RegulatedDcSupplyModel
    {
        public double NominalVoltageVolts { get; }
        public double CurrentLimitAmperes { get; }

        public RegulatedDcSupplyModel(
            double nominalVoltage,
            double currentLimit)
        {
            if (!IsPositiveFinite(nominalVoltage))
                throw new ArgumentOutOfRangeException(
                    nameof(nominalVoltage));

            if (!IsPositiveFinite(currentLimit))
                throw new ArgumentOutOfRangeException(
                    nameof(currentLimit));

            NominalVoltageVolts = nominalVoltage;
            CurrentLimitAmperes = currentLimit;
        }

        public DcSupplyResult Solve(
            double loadResistanceOhms,
            bool hasValidAcInput,
            bool switchEnabled)
        {
            if (double.IsNaN(loadResistanceOhms) ||
                loadResistanceOhms < 0 ||
                double.IsNegativeInfinity(loadResistanceOhms))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(loadResistanceOhms));
            }

            if (!hasValidAcInput || !switchEnabled)
            {
                return new DcSupplyResult(
                    DcSupplyMode.Off, 0, 0);
            }

            // Open circuit: no output current.
            if (double.IsPositiveInfinity(
                loadResistanceOhms))
            {
                return new DcSupplyResult(
                    DcSupplyMode.ConstantVoltage,
                    NominalVoltageVolts,
                    0);
            }

            // Ideal short-circuit under CC assumption.
            if (loadResistanceOhms == 0)
            {
                return new DcSupplyResult(
                    DcSupplyMode.ShortCircuit,
                    0,
                    CurrentLimitAmperes);
            }

            double requestedCurrent =
                NominalVoltageVolts /
                loadResistanceOhms;

            if (requestedCurrent <= CurrentLimitAmperes)
            {
                return new DcSupplyResult(
                    DcSupplyMode.ConstantVoltage,
                    NominalVoltageVolts,
                    requestedCurrent);
            }

            return new DcSupplyResult(
                DcSupplyMode.CurrentLimited,
                CurrentLimitAmperes *
                    loadResistanceOhms,
                CurrentLimitAmperes);
        }

        private static bool IsPositiveFinite(double x)
        {
            return !double.IsNaN(x) &&
                   !double.IsInfinity(x) &&
                   x > 0;
        }
    }
}
