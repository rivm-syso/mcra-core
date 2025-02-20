using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExposureByRouteSectionBase : SummarySection {

        protected readonly double _upperWhisker = 95;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public override bool SaveTemporaryData => true;
        public List<ExposureByRouteRecord> ExposureRecords { get; set; }
        protected double[] Percentages { get; set; }
        public List<ExposureByRoutePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];
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
    }
}
