using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureBySubstanceSectionBase : SummarySection {

        protected static List<(Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<IExternalIndividualExposure> externalIndividualExposures,
           ICollection<Compound> substances,
           IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
           bool isPerPerson
       ) {
            var exposureCollection = new List<(Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposurePath Path, Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in substances) {
                foreach (var path in paths) {
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            Path: path,
                            Substance: substance,
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.GetExposure(path, substance, isPerPerson) * kineticConversionFactor
                        )
                    ).ToList();
                    results.AddRange(exposures);
                }
            }
            var grouping = results
                .GroupBy(c => (c.Substance, c.SimulatedIndividual))
                .Select(c => (
                    Substance: c.Key.Substance,
                    SimulatedIndividual: c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            foreach (var substance in substances) {
                var exposures = grouping
                    .Where(c => c.Substance == substance)
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.Exposure
                    ))
                    .ToList();
                exposureCollection.Add((substance, exposures));
            }
            return exposureCollection;
        }
    }
}
