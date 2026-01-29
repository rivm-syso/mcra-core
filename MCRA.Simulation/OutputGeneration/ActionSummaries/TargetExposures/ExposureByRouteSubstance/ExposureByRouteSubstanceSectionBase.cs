using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureByRouteSubstanceSectionBase : SummarySection {

        protected static List<(ExposureRoute Route, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<IExternalIndividualExposure> externalIndividualExposures,
           ICollection<Compound> substances,
           IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
           bool isPerPerson
       ) {
            var exposureRouteSubstanceCollection = new List<(ExposureRoute ExposureRoute, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposureRoute Route, Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in substances) {
                foreach (var path in paths) {
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            Route: path.Route,
                            Substance: substance,
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.GetExposure(path, substance, isPerPerson) * kineticConversionFactor
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
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    var exposures = grouping
                        .Where(c => c.Route == route && c.Substance == substance)
                        .Select(c => (
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.Exposure
                        ))
                        .ToList();
                    exposureRouteSubstanceCollection.Add((route, substance, exposures));
                }
            }
            return exposureRouteSubstanceCollection;
        }
    }
}
