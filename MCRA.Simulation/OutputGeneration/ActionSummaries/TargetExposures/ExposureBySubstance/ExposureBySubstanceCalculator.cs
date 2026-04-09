using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBySubstanceCalculator {
        public static string DescriptorKey => "Substance";
        public static string DescriptorName => "substance";

        public static List<InternalExposuresByDescriptor<SubstanceContributorKey>> CalculateExposures(
            ICollection<AggregateIndividualExposure> aggregateExposures,
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
            var exposureSubstanceCollection = new List<InternalExposuresByDescriptor<SubstanceContributorKey>>();
            var results = new List<(Compound Substance, SimulatedIndividual SimulatedIndividual, double Exposure)>();
            foreach (var substance in activeSubstances) {
                var exposures = aggregateExposures
                    .Select(c => (
                        Substance: substance,
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        ) * relativePotencyFactors[substance] * membershipProbabilities[substance]
                    ))
                    .ToList();
                results.AddRange(exposures);
            }

            var grouping = results
                .GroupBy(c => (c.Substance, c.SimulatedIndividual))
                .Select(c => (
                    c.Key.Substance,
                    c.Key.SimulatedIndividual,
                    Exposure: c.Sum(r => r.Exposure)
                ))
                .ToList();
            foreach (var substance in activeSubstances) {
                var exposures = grouping
                    .Where(c => c.Substance == substance)
                    .Select(c => (
                        c.SimulatedIndividual,
                        c.Exposure
                    ))
                    .ToList();
                var internalExposures = new InternalExposuresByDescriptor<SubstanceContributorKey>() {
                    Descriptor = new SubstanceContributorKey() { Substance = substance },
                    Exposures = exposures
                };
                exposureSubstanceCollection.Add(internalExposures);
            }
            return exposureSubstanceCollection;
        }
    }
}
