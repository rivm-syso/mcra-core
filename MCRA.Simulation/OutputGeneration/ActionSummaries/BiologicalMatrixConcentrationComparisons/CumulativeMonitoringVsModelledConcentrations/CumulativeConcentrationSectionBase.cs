using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration;

public class CumulativeConcentrationSectionBase : SummarySection {

    private readonly double[] _boxPlotPercentages = [5, 10, 25, 50, 75, 90, 95];
    private double[] _percentages;
    public List<BiologicalMatrixConcentrationPercentilesRecord> Records { get; set; } = [];
    public TargetUnit TargetUnit { get; set; }

    protected BiologicalMatrixConcentrationPercentilesRecord getPercentileRecord(
        List<(double SamplingWeight, double Concentration)> exposures,
        Compound substance,
        TargetUnit targetUnit,
        string modelling
    ) {
        // All individuals
        var weightsAll = exposures
            .Select(c => c.SamplingWeight)
            .ToList();
        var percentilesAll = exposures
            .Select(c => c.Concentration)
            .PercentilesWithSamplingWeights(weightsAll, _percentages);
        var boxPlotPercentiles = exposures
            .Select(c => c.Concentration)
            .PercentilesWithSamplingWeights(weightsAll, _boxPlotPercentages)
            .ToList();

        // Positive individuals
        var positives = exposures
            .Where(r => r.Concentration > 0)
            .ToList();
        var weightsPositives = positives
            .Select(c => c.SamplingWeight)
            .ToList();
        var percentilesPositives = positives
            .Select(c => c.Concentration)
            .PercentilesWithSamplingWeights(weightsPositives, _percentages);

        var record = new BiologicalMatrixConcentrationPercentilesRecord {
            SubstanceCode = substance.Code,
            SubstanceName = substance.Name,
            Type = modelling,
            BiologicalMatrix = targetUnit.BiologicalMatrix.GetDisplayName(),
            Unit = targetUnit.ExposureUnit.GetShortDisplayName(),
            NumberOfPositives = positives.Count,
            PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
            MeanPositives = exposures.Sum(c => c.Concentration * c.SamplingWeight) / weightsPositives.Sum(),
            LowerPercentilePositives = percentilesPositives[0],
            MedianPositives = percentilesPositives[1],
            UpperPercentilePositives = percentilesPositives[2],
            LowerPercentileAll = percentilesAll[0],
            MedianAll = percentilesAll[1],
            UpperPercentileAll = percentilesAll[2],
            BoxPlotPercentiles = boxPlotPercentiles.ToList(),
            MinPositives = positives.Any() ? positives.Min(r => r.Concentration) : double.NaN,
            MaxPositives = positives.Any() ? positives.Max(r => r.Concentration) : double.NaN,
        };
        return record;
    }

    protected void setPercentages(double lowerPercentage, double upperPercentage) {
        _percentages = [lowerPercentage, 50, upperPercentage];
    }
}
