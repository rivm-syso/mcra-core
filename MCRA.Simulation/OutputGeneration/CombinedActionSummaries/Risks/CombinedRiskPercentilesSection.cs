using System.Text.RegularExpressions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedRiskPercentilesSection : SummarySection {

        public double UncertaintyLowerLimit { get; } = 2.5;
        public double UncertaintyUpperLimit { get; } = 97.5;
        public double LowerPercentile { get; set; }
        public double MedianPercentile { get; } = 50.0;
        public double UpperPercentile { get; set; }
        public double[] PercentagesConfidenceInterval { get; set; }
        public List<double> AllPercentages { get; set; }
        public List<double> DisplayPercentages { get; set; }
        public List<ExposureModelSummaryRecord> ExposureModelSummaryRecords { get; set; }
        public List<CombinedRiskPercentileRecord> CombinedExposurePercentileRecords { get; set; }
        public RiskMetricType RiskMetric { get; set; }
        public double Threshold { get; set; }
        public double LeftMarginSafetyPlot { get; set; }
        public double RightMarginSafetyPlot { get; set; }
        public bool HasUncertainty => CombinedExposurePercentileRecords?.Any(c => c.HasUncertainty) ?? false;

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
            AllPercentages = [.. riskModels.SelectMany(r => r.RiskPercentiles.Keys).Distinct()];
            DisplayPercentages = RiskMetric == RiskMetricType.HazardExposureRatio
                        ? AllPercentages.Where(v => v <= MedianPercentile).ToList()
                        : AllPercentages.Where(v => v >= MedianPercentile).ToList();

            ExposureModelSummaryRecords = [.. riskModels
                .Select(r => new ExposureModelSummaryRecord() {
                    Id = r.Code,
                    Name = r.Name,
                    Description = r.Description
                })
                .OrderBy(r => r.Name)];
            var riskUnits = riskModels.Select(r => r.ExposureUnit).Distinct();
            if (riskUnits.Count() > 1) {
                throw new Exception("Cannot combine exposures with different units");
            }
            CombinedExposurePercentileRecords = [];
            foreach (var model in riskModels) {
                CombinedExposurePercentileRecords.AddRange(
                    model.RiskPercentiles
                        .Select(r => new CombinedRiskPercentileRecord() {
                            IdModel = model.Code,
                            Name = model.Name,
                            Percentage = r.Key,
                            Risk = r.Value.Risk,
                            UncertaintyMedian = r.Value.RiskUncertainties?.Median(),
                            UncertaintyLowerBound = r.Value.RiskUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound = r.Value.RiskUncertainties?.Percentile(UncertaintyUpperLimit),
                            UncertaintyValues = r.Value.RiskUncertainties,
                        })
                        .ToList()
                );
            }
        }

        public CombinedRiskPercentileRecord GetPercentile(string idModel, double percentage) {
            return CombinedExposurePercentileRecords.FirstOrDefault(r => r.IdModel == idModel 
                && Math.Abs(r.Percentage - percentage) < 0.0000001);
        }
    }
}
