using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureBySourceSection : ExternalExposureBySourceSectionBase {
        public void Summarize(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExternalExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            Percentages = [lowerPercentage, 50, upperPercentage];
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalExposureCollections.First().ExternalIndividualDayExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;

            ExposureRecords = summarizeExposureRecords(
                externalExposureCollections,
                observedIndividualMeans,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            summarizeBoxPlotsByRoute(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                externalExposureUnit,
                isPerPerson
            );
        }
        private void summarizeBoxPlotsByRoute(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var boxPlotRecords = new List<ExternalExposureBySourcePercentileRecord>();
            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Select(id => (
                        SamplingWeight: id.IndividualSamplingWeight,
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)
                    ))
                    .ToList();
                if (exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        collection.ExposureSource,
                        exposures,
                        externalExposureUnit
                    );
                    boxPlotRecords.Add(boxPlotRecord);
                }
            }
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans.Select(id => (
                    SamplingWeight: id.IndividualSamplingWeight,
                    Exposure: id.DietaryIntakePerMassUnit
                ))
                .ToList();
                var dietaryBoxPlotRecord = getBoxPlotRecord(
                        ExposureSource.DietaryExposures,
                        oims,
                        externalExposureUnit
                    );
                boxPlotRecords.Add(dietaryBoxPlotRecord);
            }
            ExposureBoxPlotRecords = boxPlotRecords;
            ExposureUnit = externalExposureUnit;
        }

        private static ExternalExposureBySourcePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            List<(double samplingWeight, double exposure)> exposures,
            ExposureUnitTriple externalExposureUnit
        ) {
            var weights = exposures
                .Select(c => c.samplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();
            var record = new ExternalExposureBySourcePercentileRecord() {
                ExposureSource = source.GetDisplayName(),
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = externalExposureUnit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
