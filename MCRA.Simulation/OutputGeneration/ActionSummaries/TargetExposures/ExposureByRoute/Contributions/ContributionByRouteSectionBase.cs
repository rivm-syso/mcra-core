using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ContributionByRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ContributionByRouteRecord> Records { get; set; }
        public List<ContributionByRouteRecord> getContributionsRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            var result = new List<ContributionByRouteRecord>();
            foreach (var route in routes) {
                var exposures = aggregateExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(idi => (
                        SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
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
                    NumberOfDays = weights.Count,
                    Contributions = [],
                    UncertaintyLowerBound = uncertaintyLowerBound,
                    UncertaintyUpperBound = uncertaintyUpperBound
                };
                result.Add(record);
            };
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }
        protected List<ContributionByRouteRecord> SummarizeUncertainty(
             ICollection<AggregateIndividualExposure> aggregateExposures,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
             ExposureUnitTriple externalExposureUnit
        ) {
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = new List<ContributionByRouteRecord>();
            foreach (var route in routes) {
                var exposures = aggregateExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(idi => (
                        SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
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
            return [.. result.OrderByDescending(c => c.Contribution)];
        }
        protected void updateContributions(List<ContributionByRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
