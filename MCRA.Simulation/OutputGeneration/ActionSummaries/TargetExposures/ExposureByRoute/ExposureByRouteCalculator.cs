using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureByRouteCalculator {

        public static List<InternalExposuresByDescriptor<RouteContributorKey>> CalculateExposures(
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

            var exposureRouteCollection = new List<InternalExposuresByDescriptor<RouteContributorKey>>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            var results = new List<(ExposureRoute Route, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var path in paths) {
                var exposures = externalIndividualExposures
                    .Select(c => (
                        Route: path.Route,
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.ExposuresPerPath[path].Sum(p => p.Amount
                            * kineticConversionFactors[(path.Route, p.Compound)]
                            * relativePotencyFactors[p.Compound]
                            * membershipProbabilities[p.Compound]
                            / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        ))
                    ).ToList();
                results.AddRange(exposures);
            }
            var grouping = results
                .GroupBy(c => (c.Route, c.SimulatedIndividual))
                .Select(c => (
                    Route: c.Key.Route,
                    SimulatedIndividual: c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            var routes = grouping.Select(c => c.Route).ToHashSet();
            foreach (var route in routes) {
                var exposures = grouping
                    .Where(c => c.Route == route)
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.Exposure
                    ))
                    .ToList();
                var internalExposures = new InternalExposuresByDescriptor<RouteContributorKey>() {
                    Descriptor = new RouteContributorKey() { Route = route },
                    Exposures = exposures
                };
                exposureRouteCollection.Add(internalExposures);
            }
            return exposureRouteCollection;
        }
    }
}
