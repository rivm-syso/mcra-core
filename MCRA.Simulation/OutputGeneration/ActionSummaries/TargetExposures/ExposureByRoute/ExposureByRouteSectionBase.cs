using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureByRouteSectionBase : SummarySection {

        protected static List<(ExposureRoute Route, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<IExternalIndividualExposure> externalIndividualExposures,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
           bool isPerPerson
        ) {
            var exposureRouteCollection = new List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
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
                exposureRouteCollection.Add((route, exposures));
            }
            return exposureRouteCollection;
        }
    }
}
