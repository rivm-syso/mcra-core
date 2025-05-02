using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureByRouteSectionBase : SummarySection {

        protected static List<(ExposureRoute Route, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<AggregateIndividualExposure> aggregateExposures,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
           ExposureUnitTriple externalExposureUnit
       ) {
            var exposureRouteCollection = new List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
            var routes = aggregateExposures
                .SelectMany(c => c.ExternalIndividualDayExposures)
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .Select(c => c.Route)
                .ToHashSet();

            foreach (var route in routes) {
                var exposures = aggregateExposures
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.GetTotalRouteExposure(
                            route,
                            relativePotencyFactors,
                            membershipProbabilities,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit
                        )
                    ))
                    .ToList();
                exposureRouteCollection.Add((route, exposures));
            }
            return exposureRouteCollection;
        }
    }
}
