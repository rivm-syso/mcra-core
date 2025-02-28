using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureBySourceSection : SummarySection {
        public override bool SaveTemporaryData => true;

        protected static readonly double _upperWhisker = 95;

        protected static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        public List<ExternalExposureBySourceRecord> ExposureRecords { get; set; }
        public List<ExternalExposureBySourcePercentileRecord> ExposureBoxPlotRecords { get; set; }
        public ExposureUnitTriple ExposureUnit { get; set; }

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
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var result = new List<ExternalExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

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
                percentages,
                isPerPerson
            );

            ExposureBoxPlotRecords = summarizeBoxPlotsBySource(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                externalExposureUnit,
                isPerPerson
            );
        }
        private static List<ExternalExposureBySourceRecord> summarizeExposureRecords(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson
        ) {
            var result = new List<ExternalExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Select(id => (
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();
                var record = getExposureSourceRecord(
                    collection.ExposureSource,
                    exposures,
                    percentages
                );
                result.Add(record);
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans.Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.IndividualSamplingWeight
                    )).ToList();
                result.Add(getExposureSourceRecord(
                    ExposureSource.DietaryExposures,
                    oims,
                    percentages
                ));
            }
            return result;
        }

        private static ExternalExposureBySourceRecord getExposureSourceRecord(
            ExposureSource source,
            List<(double Exposure, double SamplingWeight)> exposures,
            double[] percentages
        ) {
            var weights = exposures.Where(r => r.Exposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var weightsAll = exposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var percentiles = exposures
                .Where(r => r.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new ExternalExposureBySourceRecord {
                ExposureSource = source.GetShortDisplayName(),
                Percentage = weights.Count / (double)exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total / weights.Sum(),
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                NumberOfDays = weightsAll.Count
            };
            return record;
        }

        private List<ExternalExposureBySourcePercentileRecord> summarizeBoxPlotsBySource(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var records = new List<ExternalExposureBySourcePercentileRecord>();
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
                    records.Add(boxPlotRecord);
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
                records.Add(dietaryBoxPlotRecord);
            }
            ExposureUnit = externalExposureUnit;
            return records;
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
