using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public class DistributionAggregateRouteSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        protected double[] Percentages { get; set; }

        protected List<AggregateDistributionExposureRouteTotalRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors
            // and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var route in exposureRoutes) {
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
                var record = new AggregateDistributionExposureRouteTotalRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = total,
                    Percentage = weights.Count / (double)aggregateExposures.Count * 100,
                    Mean = total / weights.Sum(),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    NumberOfDays = weights.Count,
                    Contributions = new List<double>(),
                };
                result.Add(record);
            };
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(c => c.Contribution).ToList();
        }

        protected List<AggregateDistributionExposureRouteTotalRecord> SummarizeUncertainty(
             ICollection<AggregateIndividualExposure> aggregateExposures,
             ICollection<ExposurePathType> exposureRoutes,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
             ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors
            // and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var route in exposureRoutes) {
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

                var record = new AggregateDistributionExposureRouteTotalRecord {
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
    }
}
