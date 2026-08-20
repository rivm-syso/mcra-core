using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySubstanceUpperSection : InternalExposureUpperContributionSectionBase<SubstanceContributorKey, ContributionBySubstanceRecord> {
        public override string DescriptorKey => ExposureBySubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySubstanceCalculator.DescriptorName;
        public void Summarize<T>(
            ICollection<T> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
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

            Records = summarize(
                exposureCollection,
                outputStratifier,
                percentageForUpperTail,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                true
            );
        }

        public void SummarizeUncertainty<T>(
            ICollection<T> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
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

            var records = summarizeUncertainty(exposureCollection, outputStratifier, percentageForUpperTail);
            updateContributions(Records, records);
        }
    }
}