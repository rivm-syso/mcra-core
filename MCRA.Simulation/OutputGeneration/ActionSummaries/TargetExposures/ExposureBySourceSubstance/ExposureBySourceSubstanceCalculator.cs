using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBySourceSubstanceCalculator {
        public static string DescriptorKey => "SourceSubstance";
        public static string DescriptorName => "source and substance";

        public static List<InternalExposuresByDescriptor<SourceSubstanceContributorKey>> CalculateExposures(
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
            var exposureSourceSubstanceCollection = new List<InternalExposuresByDescriptor<SourceSubstanceContributorKey>>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();
            var results = new List<(ExposureSource Source, Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in activeSubstances) {
                foreach (var path in paths) {
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            Source: path.Source,
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
                .GroupBy(c => (c.Source, c.Substance, c.SimulatedIndividual))
                .Select(c => (
                    c.Key.Source,
                    c.Key.Substance,
                    c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            var sources = grouping.Select(c => c.Source).ToHashSet();
            foreach (var substance in activeSubstances) {
                foreach (var source in sources) {
                    var exposures = grouping
                        .Where(c => c.Source == source && c.Substance == substance)
                        .Select(c => (
                            c.SimulatedIndividual,
                            c.Exposure
                        ))
                        .ToList();
                    var internalExposures = new InternalExposuresByDescriptor<SourceSubstanceContributorKey>() {
                        Descriptor = new SourceSubstanceContributorKey() { Source = source, Substance = substance },
                        Exposures = exposures
                    };
                    exposureSourceSubstanceCollection.Add(internalExposures);
                }
            }
            return exposureSourceSubstanceCollection;
        }
    }
}
