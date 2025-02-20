using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

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
            TargetUnit targetUnit,
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

            ExposureRecords = SummarizeExposures(
                externalExposureCollections,
                observedIndividualMeans,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                targetUnit,
                externalExposureUnit,
                isPerPerson
            );

            summarizeBoxPlotsByRoute(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                externalExposureUnit,
                targetUnit,
                isPerPerson
            );
        }
        private void summarizeBoxPlotsByRoute(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
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
                        targetUnit
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
                        targetUnit
                    );
                boxPlotRecords.Add(dietaryBoxPlotRecord);
            }
            ExposureBoxPlotRecords = boxPlotRecords;
            TargetUnit = targetUnit;
        }

        private static ExternalExposureBySourcePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            List<(double samplingWeight, double exposure)> exposures,
            TargetUnit targetUnit
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
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
