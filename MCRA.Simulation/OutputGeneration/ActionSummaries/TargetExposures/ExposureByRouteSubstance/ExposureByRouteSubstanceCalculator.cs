using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByRouteSubstanceCalculator {
        public static string DescriptorKey => "RouteSubstance";
        public static string DescriptorName => "route and substance";

        public static List<InternalExposuresByDescriptor<RouteSubstanceContributorKey>> CalculateExposures(
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

            var exposureRouteSubstanceCollection = new List<InternalExposuresByDescriptor<RouteSubstanceContributorKey>>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposureRoute Route, Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in activeSubstances) {
                foreach (var path in paths) {
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            Route: path.Route,
                            Substance: substance,
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.GetExposure(path, substance, isPerPerson) 
                                * kineticConversionFactor * relativePotencyFactors[substance] * membershipProbabilities[substance]
                        )
                    ).ToList();
                    results.AddRange(exposures);
                }
            }
            var grouping = results
                .GroupBy(c => (c.Route, c.Substance, c.SimulatedIndividual))
                .Select(c => (
                    Route: c.Key.Route,
                    Substance: c.Key.Substance,
                    SimulatedIndividual: c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            var routes = grouping.Select(c => c.Route).ToHashSet();
            foreach (var substance in activeSubstances) {
                foreach (var route in routes) {
                    var exposures = grouping
                        .Where(c => c.Route == route && c.Substance == substance)
                        .Select(c => (
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.Exposure
                        ))
                        .ToList();
                    var internalExposures = new InternalExposuresByDescriptor<RouteSubstanceContributorKey>() {
                        Descriptor = new RouteSubstanceContributorKey() { Route = route, Substance = substance.Name },
                        Exposures = exposures
                    };
                    exposureRouteSubstanceCollection.Add(internalExposures);
                }
            }
            return exposureRouteSubstanceCollection;
        }
    }
}
