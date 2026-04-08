using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBySourceCalculator {
        public static string DescriptorKey => "Source";
        public static string DescriptorName => "source";

        public static List<InternalExposuresByDescriptor<SourceContributorKey>> CalculateExposures(
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
            var exposureSourceCollection = new List<InternalExposuresByDescriptor<SourceContributorKey>>(); ;
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
                            * kineticConversionFactors[(path.Route, p.Compound)]
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
                var internalExposures = new InternalExposuresByDescriptor<SourceContributorKey>() {
                    Descriptor = new SourceContributorKey() { Source = source },
                    Exposures = exposures
                };
                exposureSourceCollection.Add(internalExposures);
            }
            return exposureSourceCollection;
        }
    }
}
