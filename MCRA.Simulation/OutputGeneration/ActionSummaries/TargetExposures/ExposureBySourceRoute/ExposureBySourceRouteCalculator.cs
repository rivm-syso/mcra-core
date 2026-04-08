using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureBySourceRouteCalculator {
        public static string DescriptorKey => "SourceRoute";
        public static string DescriptorName => "source and route";

        public static List<InternalExposuresByDescriptor<SourceRouteContributorKey>> CalculateExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            var exposurePathCollection = new List<InternalExposuresByDescriptor<SourceRouteContributorKey>>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            foreach (var path in paths) {
                var exposures = externalIndividualExposures
                .Select(c => (
                    SimulatedIndividual: c.SimulatedIndividual,
                    Exposure: c.ExposuresPerPath[path].Sum(r => r.Amount
                        * kineticConversionFactors[(path.Route, r.Compound)]
                        * relativePotencyFactors[r.Compound]
                        * membershipProbabilities[r.Compound]
                        / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                    ))
                ).ToList();
                var internalExposures = new InternalExposuresByDescriptor<SourceRouteContributorKey>() {
                    Descriptor = new SourceRouteContributorKey() { Route = path.Route, Source = path.Source},
                    Exposures = exposures
                };
                exposurePathCollection.Add(internalExposures);
            }
            return exposurePathCollection;
        }
    }
}
