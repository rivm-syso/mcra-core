using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.CombinedActionSummaries;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedRiskPercentilesSection : CombinedPercentilesSectionBase {
        public double LowerPercentile { get; set; }
        public double MedianPercentile { get; } = 50.0;
        public double UpperPercentile { get; set; }
        public double[] PercentagesConfidenceInterval { get; set; }
        public List<double> DisplayPercentages { get; set; }
        public RiskMetricType RiskMetric { get; set; }
        public double Threshold { get; set; }
        public double LeftMarginSafetyPlot { get; set; }
        public double RightMarginSafetyPlot { get; set; }
        public bool HasUncertainty => CombinedPercentileRecords?.Any(c => c.HasUncertainty) ?? false;

        public void Summarize(ICollection<RiskModel> riskModels, RiskMetricType riskMetric) {
            RiskMetric = riskMetric;
            Threshold = riskMetric == RiskMetricType.HazardExposureRatio
                ? riskModels.FirstOrDefault().ThresholdMarginOfExposure
                : 1.0 / riskModels.FirstOrDefault().ThresholdMarginOfExposure;
            LeftMarginSafetyPlot = riskModels.FirstOrDefault().LeftMarginSafetyPlot;
            RightMarginSafetyPlot = riskModels.FirstOrDefault().RightMarginSafetyPlot;

            var confidenceInterval = riskModels.FirstOrDefault().ConfidenceInterval;
            LowerPercentile = (100 - confidenceInterval) / 2;
            UpperPercentile = 100 - LowerPercentile;

            // All percentages include both sides of the distribution, e.g., p0.1 and p99.9, used for safety chart.
            // Display percentages are limited to the half until p50.0, which agrees with the risk metric.
            Percentages = [.. riskModels.SelectMany(r => r.RiskPercentiles.Keys).Distinct()];
            DisplayPercentages = RiskMetric == RiskMetricType.HazardExposureRatio
                        ? Percentages.Where(v => v <= MedianPercentile).ToList()
                        : Percentages.Where(v => v >= MedianPercentile).ToList();

            ModelSummaryRecords = [.. riskModels
                .Select(r => new ModelSummaryRecord(
                    Id : r.Code,
                    Name : r.Name,
                    Description : r.Description
                ))
                .OrderBy(r => r.Name)];
            var riskUnits = riskModels.Select(r => r.ExposureUnit).Distinct();
            if (riskUnits.Count() > 1) {
                throw new Exception("Cannot combine exposures with different units");
            }
            CombinedPercentileRecords = [];
            foreach (var model in riskModels) {
                CombinedPercentileRecords.AddRange(
                    model.RiskPercentiles
                        .Select(r => new CombinedPercentileRecord(
                            IdModel: model.Code,
                            Name: model.Name,
                            Percentage: r.Key,
                            Value: r.Value.Risk,
                            UncertaintyMedian: r.Value.RiskUncertainties?.Median(),
                            UncertaintyLowerBound: r.Value.RiskUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound: r.Value.RiskUncertainties?.Percentile(UncertaintyUpperLimit),
                            UncertaintyValues: r.Value.RiskUncertainties
                        ))
                        .ToList()
                );
            }
        }
    }
}
