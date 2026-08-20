using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySubstanceTotalSection : InternalExposureContributionSectionBase<SubstanceContributorKey, ContributionBySubstanceRecord> {
        public override string DescriptorKey => ExposureBySubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySubstanceCalculator.DescriptorName;

        public void Summarize<T>(
            ICollection<T> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) where T: AggregateIndividualExposureBase {
            var exposureCollection = ExposureBySubstanceCalculator.CalculateExposures(
                aggregates,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            Records = summarize(
                exposureCollection,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                outputStratifier
            );
        }

        public void SummarizeUncertainty<T>(
            ICollection<T> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            bool isPerPerson
        ) where T : AggregateIndividualExposureBase {
            var exposureCollection = ExposureBySubstanceCalculator.CalculateExposures(
                aggregates,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );
            var records = summarizeUncertainty(exposureCollection, outputStratifier);
            updateContributions(Records, records);
        }
    }
}