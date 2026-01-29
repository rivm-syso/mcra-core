using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ContributionByRouteSectionBase : ExposureByRouteSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ContributionByRouteRecord> Records { get; set; }
        public List<ContributionByRouteRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            //var cancelToken = ProgressState?.CancellationToken ?? new();
            //var routesxx = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            var result = new List<ContributionByRouteRecord>();

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Route, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionByRouteRecord(
                        Route,
                        Exposures,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ContributionByRouteRecord getContributionByRouteRecord(
            ExposureRoute route,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ContributionByRouteRecord {
                ExposureRoute = route.GetShortDisplayName(),
                Contribution = total,
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            return record;
        }

        protected List<ContributionByRouteRecord> SummarizeUncertainty(
             ICollection<IExternalIndividualExposure> externalIndividualExposures,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
             bool isPerPerson
        ) {
            //var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            //var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = new List<ContributionByRouteRecord>();
            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Route, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionByRouteRecord {
                        ExposureRoute = Route.GetShortDisplayName(),
                        Contribution = Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                    };
                    result.Add(record);
                };
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected void UpdateContributions(List<ContributionByRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
