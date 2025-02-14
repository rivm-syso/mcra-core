using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    public class DistributionByRouteSectionBase : SummarySection {
        protected readonly double _upperWhisker = 95;
        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public override bool SaveTemporaryData => true;
        public List<ExposureByRouteRecord> ExposureRecords { get; set; }

        public List<ContributionByRouteRecord> ContributionRecords { get; set; }
        protected double[] Percentages { get; set; }
        public List<PercentilesRecordBase> ExposureBoxPlotRecords { get; set; } = [];
        public double? RestrictedUpperPercentile { get; set; }

        public TargetUnit TargetUnit { get; set; }

        protected List<ExposureByRouteRecord> SummarizeExposures(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors
            // and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExposureByRouteRecord>();
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

                var weights = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentiles = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, Percentages);
                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
                var weightsAll = exposures.Select(idi => idi.SamplingWeight).ToList();
                var percentilesAll = exposures
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weightsAll, Percentages);
                var record = new ExposureByRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Percentage = weights.Count / (double)aggregateExposures.Count * 100,
                    Mean = total / weights.Sum(),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    NumberOfDays = weights.Count,
                };
                result.Add(record);
            };
            result.TrimExcess();
            return result;
        }
        protected List<ContributionByRouteRecord> SummarizeContributions(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ContributionByRouteRecord>();
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

                var weightsAll = exposures
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var weights = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
                var record = new ContributionByRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = total,
                    Percentage = weights.Count / (double)aggregateExposures.Count * 100,
                    Mean = total / weightsAll.Sum(),
                    NumberOfDays = weightsAll.Count,
                    Contributions = [],
                };
                result.Add(record);
            };
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected List<ContributionByRouteRecord> SummarizeUncertainty(
             ICollection<AggregateIndividualExposure> aggregateExposures,
             ICollection<ExposureRoute> routes,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
             ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors
            // and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ContributionByRouteRecord>();
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

                var record = new ContributionByRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight),
                };
                result.Add(record);
            };
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(c => c.Contribution).ToList();
        }

        protected void UpdateContributions(List<ContributionByRouteRecord> records) {
            foreach (var record in ContributionRecords) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }

        protected static void getBoxPlotRecord(
            List<PercentilesRecordBase> result,
            string description,
            List<(double samplingWeight, double exposure)> exposures,
            TargetUnit targetUnit
        ) {
            if (exposures.Any(c => c.exposure > 0)) {
                var weights = exposures
                    .Select(c => c.samplingWeight)
                    .ToList();
                var allExposures = exposures
                    .Select(c => c.exposure)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var outliers = allExposures
                        .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                            || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => c)
                        .ToList();

                var record = new PercentilesRecordBase() {
                    Description = description,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    Percentiles = percentiles,
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / exposures.Count,
                    Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count,
                };
                result.Add(record);
            }
        }
    }
}
