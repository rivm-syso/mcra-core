using System.Text.RegularExpressions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedDietaryExposurePercentilesSection : SummarySection {

        public ExternalExposureUnit ExposureUnit { get; set; }

        public double UncertaintyLowerLimit { get; set; } = 2.5;

        public double UncertaintyUpperLimit { get; set; } = 97.5;

        public List<double> Percentages { get; set; }

        public List<ExposureModelSummaryRecord> ExposureModelSummaryRecords { get; set; }

        public List<CombinedRiskPercentileRecord> CombinedExposurePercentileRecords { get; set; }

        public void Summarize(ICollection<DietaryExposureModel> exposureModels) {
            Percentages = exposureModels.SelectMany(r => r.DietaryExposurePercentiles.Keys).Distinct().ToList();
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
            ExposureUnit = exposureUnits.FirstOrDefault();
            CombinedExposurePercentileRecords = [];
            foreach (var model in exposureModels) {
                CombinedExposurePercentileRecords.AddRange(
                    model.DietaryExposurePercentiles
                        .Select(r => new CombinedRiskPercentileRecord() {
                            IdModel = model.Code,
                            Percentage = r.Key,
                            Risk = r.Value.Exposure,
                            UncertaintyMedian = r.Value.ExposureUncertainties?.Median(),
                            UncertaintyLowerBound = r.Value.ExposureUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound = r.Value.ExposureUncertainties?.Percentile(UncertaintyUpperLimit),
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
