using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceSection : ContributionBySourceSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureBySourceRecord> ExposureRecords { get; set; }
        public List<ExposureBySourcePercentileRecord> ExposureBoxPlotRecords { get; set; }
        public ExposureUnitTriple ExposureUnit { get; set; }

        public void Summarize(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalExposureCollections.First().ExternalIndividualDayExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            ExposureUnit = externalExposureUnit;

            var exposuresBySources = new List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposuresPerSourceRoute = GetExposuresPerSource(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        isPerPerson,
                        collection,
                        route
                    );
                    exposuresBySources.AddRange(exposuresPerSourceRoute);
                }
            }
            if (dietaryIndividualDayIntakes != null) {
                var observedIndividualMeans = GetInternalObservedIndividualMeans(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    isPerPerson
                );
                exposuresBySources.AddRange(observedIndividualMeans);
            }

            ExposureRecords = exposuresBySources.GroupBy(c => c.Source)
                .Select(gr => getExposureSourceRecord(
                    gr.Key,
                    gr.GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            SamplingWeight: c.First().SamplingWeight,
                            Exposure: c.Sum(r => r.Exposure)
                        )).ToList(),
                    percentages
                )).ToList();

            ExposureBoxPlotRecords = exposuresBySources.GroupBy(c => c.Source)
                .Select(gr => getBoxPlotRecord(
                    gr.Key,
                    gr.GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            SamplingWeight: c.First().SamplingWeight,
                            Exposure: c.Sum(r => r.Exposure)
                        )).ToList(),
                    externalExposureUnit
                )).ToList();
        }

        private ExposureBySourceRecord getExposureSourceRecord(
            ExposureSource source,
            List<(double SamplingWeight, double Exposure)> exposures,
            double[] percentages
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = exposures
                .Select(idi => idi.SamplingWeight)
                .ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var record = new ExposureBySourceRecord {
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
                NumberOfDays = weights.Count,
            };
            return record;
        }

        private static ExposureBySourcePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            List<(double SamplingWeight, double Exposure)> exposures,
            ExposureUnitTriple unit
        ) {
            var weights = exposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.Exposure)
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
            var record = new ExposureBySourcePercentileRecord() {
                ExposureSource = source.GetDisplayName(),
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = unit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
