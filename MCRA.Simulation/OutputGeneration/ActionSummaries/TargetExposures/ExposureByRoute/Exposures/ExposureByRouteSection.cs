using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        private static readonly double _upperWhisker = 95;

        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureByRouteRecord> ExposureRecords { get; set; }
        public List<ExposureByRoutePercentileRecord> ExposureBoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();

            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(aggregateExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            ExposureRecords = summarizeExposureRecords(
                aggregateExposures,
                routes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit,
                percentages
            );

            ExposureBoxPlotRecords = summarizeBoxPlotsByRoute(
                aggregateExposures,
                routes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit,
                targetUnit
            );
        }

        private static List<ExposureByRouteRecord> summarizeExposureRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            List<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double[] percentages
        ) {
            var result = new List<ExposureByRouteRecord>();
            foreach (var route in routes) {
                var exposures = aggregateExposures
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        Exposure: idi.GetTotalRouteExposure(
                            route,
                            relativePotencyFactors,
                            membershipProbabilities,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        )
                    ))
                    .ToList();

                var weightsAll = exposures
                    .Select(idi => idi.SamplingWeight)
                    .ToList();
                var percentilesAll = exposures
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);

                var weightsPositives = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesPositives = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weightsPositives, percentages);

                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);

                var record = new ExposureByRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Percentage = weightsPositives.Count / (double)aggregateExposures.Count * 100,
                    MeanAll = total / weightsAll.Sum(),
                    Mean = total / weightsPositives.Sum(),
                    Percentile25 = percentilesPositives[0],
                    Median = percentilesPositives[1],
                    Percentile75 = percentilesPositives[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    NumberOfDays = weightsPositives.Count,
                };
                result.Add(record);
            };
            return result;
        }

        private List<ExposureByRoutePercentileRecord> summarizeBoxPlotsByRoute(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            var result = new List<ExposureByRoutePercentileRecord>();
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            foreach (var route in routes) {
                var exposures = aggregateExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        Exposure: idi.GetTotalRouteExposure(
                            route,
                            relativePotencyFactors,
                            membershipProbabilities,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        )
                    ))
                    .ToList();
                if (exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        route,
                        exposures,
                        targetUnit
                    );
                    result.Add(boxPlotRecord);
                }
            }
            return result;
        }
        private static ExposureByRoutePercentileRecord getBoxPlotRecord(
            ExposureRoute route,
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

            var record = new ExposureByRoutePercentileRecord() {
                ExposureRoute = route != ExposureRoute.Undefined
                    ? route.GetDisplayName() : null,
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
