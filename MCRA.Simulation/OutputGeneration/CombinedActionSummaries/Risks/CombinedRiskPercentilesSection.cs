using MCRA.Data.Compiled.Objects;
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

        public List<CombinedExposurePercentileRecord> CombinedExposurePercentileRecords { get; set; }

        public void Summarize(ICollection<RiskModel> exposureModels) {
            Percentages = exposureModels.SelectMany(r => r.RiskPercentiles.Keys).Distinct().ToList();
            ExposureModelSummaryRecords = exposureModels
                .Select(r => new ExposureModelSummaryRecord() {
                    Id = r.Code,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToList();
            var exposureUnits = exposureModels.Select(r => r.ExposureUnit).Distinct();
            if (exposureUnits.Count() > 1) {
                throw new Exception("Cannot combine exposures with different units");
            }
            CombinedExposurePercentileRecords = new List<CombinedExposurePercentileRecord>();
            foreach (var model in exposureModels) {
                CombinedExposurePercentileRecords.AddRange(
                    model.RiskPercentiles
                        .Select(r => new CombinedExposurePercentileRecord() {
                            IdModel = model.Code,
                            Name = model.Name,
                            Percentage = r.Key,
                            Exposure = r.Value.MarginOfExposure,
                            UncertaintyMedian = r.Value.MarginOfExposureUncertainties?.Median(),
                            UncertaintyLowerBound = r.Value.MarginOfExposureUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound = r.Value.MarginOfExposureUncertainties?.Percentile(UncertaintyUpperLimit),
                            UncertaintyValues = r.Value.MarginOfExposureUncertainties,
                        })
                        .ToList()
                );
            }
        }

        public CombinedExposurePercentileRecord GetPercentile(string idModel, double percentage) {
            return CombinedExposurePercentileRecords.FirstOrDefault(r => r.IdModel == idModel && r.Percentage == percentage);
        }
    }
}
