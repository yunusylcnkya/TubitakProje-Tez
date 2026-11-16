
using System;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using Thesis.Physics;

[Serializable]
public class DcMeasurementSample
{
    public string experimentId = "D01";

    public bool openCircuit;

    [Min(0.001f)]
    public float loadResistanceOhms = 1200f;

    [Header("Measured Values")]
    public bool measurementCompleted;

    [Min(0f)]
    public float measuredVoltageVolts;

    [Min(0f)]
    public float measuredCurrentAmperes;
}

public sealed class DcSupplyValidationRecorder : MonoBehaviour
{
    [Header("Simulation Parameters")]
    [SerializeField, Min(0.01f)]
    private float nominalVoltageVolts = 24f;

    [SerializeField, Min(0.01f)]
    private float currentLimitAmperes = 5f;

    [Header("Physical Measurement Dataset")]
    [SerializeField]
    private DcMeasurementSample[] samples =
    {
        new DcMeasurementSample
        {
            experimentId = "D01",
            openCircuit = true
        },

        new DcMeasurementSample
        {
            experimentId = "D02",
            loadResistanceOhms = 1200f
        },

        new DcMeasurementSample
        {
            experimentId = "D03",
            loadResistanceOhms = 470f
        },

        new DcMeasurementSample
        {
            experimentId = "D04",
            loadResistanceOhms = 240f
        }
    };

    [ContextMenu("Analyze and Export DC Measurements")]
    public void AnalyzeAndExport()
    {
        try
        {
            AnalyzeInternal();
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "DC validation failed:\n" + ex,
                this);
        }
    }

    private void AnalyzeInternal()
    {
        var model = new RegulatedDcSupplyModel(
            nominalVoltageVolts,
            currentLimitAmperes);

        var csv = new StringBuilder();

        csv.AppendLine(
            "Experiment,ResistanceOhms," +
            "MeasuredVoltageV,PredictedVoltageV," +
            "VoltageErrorV,MeasuredCurrentA," +
            "PredictedCurrentA,CurrentErrorA," +
            "PredictedMode");

        int count = 0;

        double voltageAbsoluteErrorSum = 0;
        double voltageSquaredErrorSum = 0;

        double currentAbsoluteErrorSum = 0;
        double currentSquaredErrorSum = 0;

        if (samples == null)
            return;

        foreach (DcMeasurementSample sample in samples)
        {
            if (sample == null)
                continue;

            double resistance = sample.openCircuit
                ? double.PositiveInfinity
                : sample.loadResistanceOhms;

            if (!sample.openCircuit &&
                (resistance <= 0 ||
                 double.IsNaN(resistance) ||
                 double.IsInfinity(resistance)))
            {
                Debug.LogWarning(
                    "Invalid resistance: " +
                    sample.experimentId, this);
                continue;
            }

            // The experiment assumes the PSU is ON
            // and its AC input is valid.
            DcSupplyResult predicted = model.Solve(
                resistance, true, true);

            string measuredVoltage = "";
            string measuredCurrent = "";
            string voltageErrorText = "";
            string currentErrorText = "";

            if (sample.measurementCompleted)
            {
                double vm =
                    sample.measuredVoltageVolts;

                double im =
                    sample.measuredCurrentAmperes;

                if (!IsValidMeasurement(vm) ||
                    !IsValidMeasurement(im))
                {
                    Debug.LogWarning(
                        "Invalid measurement: " +
                        sample.experimentId, this);
                    continue;
                }

                double voltageError =
                    predicted.VoltageVolts - vm;

                double currentError =
                    predicted.CurrentAmperes - im;

                voltageAbsoluteErrorSum +=
                    Math.Abs(voltageError);

                voltageSquaredErrorSum +=
                    voltageError * voltageError;

                currentAbsoluteErrorSum +=
                    Math.Abs(currentError);

                currentSquaredErrorSum +=
                    currentError * currentError;

                count++;

                measuredVoltage = N(vm);
                measuredCurrent = N(im);

                voltageErrorText = N(voltageError);
                currentErrorText = N(currentError);
            }

            string resistanceText =
                sample.openCircuit
                    ? "OPEN"
                    : N(resistance);

            csv.AppendLine(
                sample.experimentId + "," +
                resistanceText + "," +
                measuredVoltage + "," +
                N(predicted.VoltageVolts) + "," +
                voltageErrorText + "," +
                measuredCurrent + "," +
                N(predicted.CurrentAmperes) + "," +
                currentErrorText + "," +
                predicted.Mode);
        }

        string directory = Path.Combine(
            Application.persistentDataPath,
            "ThesisResearch");

        Directory.CreateDirectory(directory);

        string filename =
            "DcSupplyValidation_" +
            DateTime.UtcNow.ToString(
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture) +
            "_" +
            Guid.NewGuid().ToString("N").Substring(0, 8) +
            ".csv";

        string path = Path.Combine(
            directory, filename);

        File.WriteAllText(
            path,
            csv.ToString(),
            new UTF8Encoding(true));

        if (count > 0)
        {
            double voltageMae =
                voltageAbsoluteErrorSum / count;

            double voltageRmse = Math.Sqrt(
                voltageSquaredErrorSum / count);

            double currentMae =
                currentAbsoluteErrorSum / count;

            double currentRmse = Math.Sqrt(
                currentSquaredErrorSum / count);

            Debug.Log(
                "THESIS VALIDATION | " +
                "Samples: " + count +
                " | Voltage MAE: " +
                N(voltageMae) + " V" +
                " | Voltage RMSE: " +
                N(voltageRmse) + " V" +
                " | Current MAE: " +
                N(currentMae) + " A" +
                " | Current RMSE: " +
                N(currentRmse) + " A",
                this);
        }
        else
        {
            Debug.LogWarning(
                "No physical measurements entered yet. " +
                "CSV contains theoretical predictions only.",
                this);
        }

        Debug.Log(
            "THESIS CSV EXPORTED: " + path,
            this);
    }

    private static bool IsValidMeasurement(double value)
    {
        return !double.IsNaN(value) &&
               !double.IsInfinity(value) &&
               value >= 0;
    }

    private static string N(double value)
    {
        return value.ToString(
            "G17",
            CultureInfo.InvariantCulture);
    }
}
