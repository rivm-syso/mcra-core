using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExternalExposureBySourceSectionBase : SummarySection {

        protected static List<(ExposureSource Source, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> CalculateExposures(
           ICollection<IExternalIndividualExposure> externalIndividualExposures,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           bool isPerPerson
       ) {
            var exposureSourceCollection = new List<(ExposureSource ExposureSource, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposureSource Source, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var path in paths) {
                var exposures = externalIndividualExposures
                    .Select(c => (
                        Source: path.Source,
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.ExposuresPerPath[path].Sum(p => p.Amount
                            * relativePotencyFactors[p.Compound]
                            * membershipProbabilities[p.Compound]
                            / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        ))
                    ).ToList();
                results.AddRange(exposures);
            }
            var grouping = results
                .GroupBy(c => (c.Source, c.SimulatedIndividual))
                .Select(c => (
                    Source: c.Key.Source,
                    SimulatedIndividual: c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            var sources = grouping.Select(c => c.Source).ToHashSet();
            foreach (var source in sources) {
                var exposures = grouping
                    .Where(c => c.Source == source)
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.Exposure
                    ))
                    .ToList();
                exposureSourceCollection.Add((source, exposures));
            }
            return exposureSourceCollection;
        }
    }
}
