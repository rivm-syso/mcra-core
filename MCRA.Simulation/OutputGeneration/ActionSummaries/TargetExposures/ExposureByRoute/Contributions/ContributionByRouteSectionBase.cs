using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public static class ContributionByRouteSectionBase {


        public static List<ContributionByRouteRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionByRouteRecord>();

            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var collection in exposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionByRouteRecord(
                        collection,
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
            InternalExposuresByDescriptor<RouteContributorKey> collection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = collection.Exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = collection.Exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ContributionByRouteRecord {
                ExposureRoute = collection.Descriptor.Route.GetShortDisplayName(),
                Contribution = total,
                Percentage = weights.Count / (double)collection.Exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            return record;
        }

        public static List<ContributionByRouteRecord> SummarizeUncertainty(
             ICollection<IExternalIndividualExposure> externalIndividualExposures,
             IDictionary<Compound, double> relativePotencyFactors,
             IDictionary<Compound, double> membershipProbabilities,
             IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
             bool isPerPerson
        ) {
            var result = new List<ContributionByRouteRecord>();
            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var collection in exposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionByRouteRecord {
                        ExposureRoute = collection.Descriptor.Route.GetShortDisplayName(),
                        Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                    };
                    result.Add(record);
                };
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        public static void UpdateContributions(List<ContributionByRouteRecord> updateRecords, List<ContributionByRouteRecord> records) {
            foreach (var record in updateRecords) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
