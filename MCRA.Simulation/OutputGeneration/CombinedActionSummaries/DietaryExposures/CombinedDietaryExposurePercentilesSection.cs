using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedDietaryExposurePercentilesSection : SummarySection {

        public ExposureUnit ExposureUnit { get; set; }

        public double UncertaintyLowerLimit { get; set; } = 2.5;

        public double UncertaintyUpperLimit { get; set; } = 97.5;

        public List<double> Percentages { get; set; }

        public List<ExposureModelSummaryRecord> ExposureModelSummaryRecords { get; set; }

        public List<CombinedExposurePercentileRecord> CombinedExposurePercentileRecords { get; set; }

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
            CombinedExposurePercentileRecords = new List<CombinedExposurePercentileRecord>();
            foreach (var model in exposureModels) {
                CombinedExposurePercentileRecords.AddRange(
                    model.DietaryExposurePercentiles
                        .Select(r => new CombinedExposurePercentileRecord() {
                            IdModel = model.Code,
                            Percentage = r.Key,
                            Exposure = r.Value.Exposure,
                            UncertaintyMedian = r.Value.ExposureUncertainties?.Median(),
                            UncertaintyLowerBound = r.Value.ExposureUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound = r.Value.ExposureUncertainties?.Percentile(UncertaintyUpperLimit),
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
