using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.CombinedActionSummaries;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Represents a collection of all simulation data that has been post-processed for visualization.
    /// </summary>
    public sealed class CombinedDietaryExposurePercentilesSection : CombinedPercentilesSectionBase {
        public ExternalExposureUnit ExposureUnit { get; set; }

        public void Summarize(ICollection<DietaryExposureModel> exposureModels) {
            Percentages = [.. exposureModels.SelectMany(r => r.DietaryExposurePercentiles.Keys).Distinct()];
            ModelSummaryRecords = [.. exposureModels
                .Select(r => new ModelSummaryRecord(
                    Id : r.Code,
                    Name : r.Name,
                    Description : r.Description
                ))
                .OrderBy(r => r.Name)];
            var exposureUnits = exposureModels.Select(r => r.ExposureUnit).Distinct();
            if (exposureUnits.Count() > 1) {
                throw new Exception("Cannot combine exposures with different units");
            }
            ExposureUnit = exposureUnits.FirstOrDefault();
            CombinedPercentileRecords = [];
            foreach (var model in exposureModels) {
                CombinedPercentileRecords.AddRange(
                    model.DietaryExposurePercentiles
                        .Select(r => new CombinedPercentileRecord(
                            IdModel: model.Code,
                            Name: model.Name,
                            Percentage: r.Key,
                            Value: r.Value.Exposure,
                            UncertaintyMedian: r.Value.ExposureUncertainties?.Median(),
                            UncertaintyLowerBound: r.Value.ExposureUncertainties?.Percentile(UncertaintyLowerLimit),
                            UncertaintyUpperBound: r.Value.ExposureUncertainties?.Percentile(UncertaintyUpperLimit),
                            UncertaintyValues: r.Value.ExposureUncertainties?.ToList()
                        ))
                        .ToList()
                );
            }
        }
    }
}
