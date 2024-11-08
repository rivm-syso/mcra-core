using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedRiskPercentilesSection : SummarySection {

        public double UncertaintyLowerLimit { get; set; } = 2.5;

        public double UncertaintyUpperLimit { get; set; } = 97.5;

        public List<double> Percentages { get; set; }

        public List<ExposureModelSummaryRecord> ExposureModelSummaryRecords { get; set; }

        public List<CombinedRiskPercentileRecord> CombinedExposurePercentileRecords { get; set; }

        public RiskMetricType RiskMetric { get; set; }

        public void Summarize(ICollection<RiskModel> riskModels, RiskMetricType riskMetric) {
            RiskMetric = riskMetric;
            Percentages = riskModels.SelectMany(r => r.RiskPercentiles.Keys).Distinct().ToList();
            ExposureModelSummaryRecords = riskModels
                .Select(r => new ExposureModelSummaryRecord() {
                    Id = r.Code,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToList();
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
            return CombinedExposurePercentileRecords.FirstOrDefault(r => r.IdModel == idModel && r.Percentage == percentage);
        }
    }
}
