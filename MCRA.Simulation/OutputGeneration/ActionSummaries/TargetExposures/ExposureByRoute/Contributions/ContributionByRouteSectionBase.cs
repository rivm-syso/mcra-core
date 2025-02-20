using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ContributionByRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ContributionByRouteRecord> ContributionRecords { get; set; }
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
    }
}
