using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureBySourceSubstanceSectionBase : SummarySection {

        protected static List<(ExposureSource Source, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<IExternalIndividualExposure> externalIndividualExposures,
           ICollection<Compound> substances,
           IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
           bool isPerPerson
       ) {
            var exposureSourceSubstanceCollection = new List<(ExposureSource ExposureSource, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposureSource Source, Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in substances) {
                foreach (var path in paths) {
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            Source: path.Source,
                            Substance: substance,
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.ExposuresPerPath[path].First(c => c.Compound.Code == substance.Code).Amount
                                * kineticConversionFactors[(path.Route, substance)]
                                / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        )
                    ).ToList();
                    results.AddRange(exposures);
                }
            }
            var grouping = results
                .GroupBy(c => (c.Source, c.Substance, c.SimulatedIndividual))
                .Select(c => (
                    Source: c.Key.Source,
                    Substance: c.Key.Substance,
                    SimulatedIndividual: c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            var sources = grouping.Select(c => c.Source).ToHashSet();
            foreach (var substance in substances) {
                foreach (var source in sources) {
                    var exposures = grouping
                        .Where(c => c.Source == source && c.Substance == substance)
                        .Select(c => (
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.Exposure
                        ))
                        .ToList();
                    exposureSourceSubstanceCollection.Add((source, substance, exposures));
                }
            }
            return exposureSourceSubstanceCollection;
        }
    }
}
