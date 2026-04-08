using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySubstanceUpperSection : InternalExposureUpperContributionSectionBase<SubstanceContributorKey, ContributionBySubstanceRecord> {
        public override string DescriptorKey => ExposureBySubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySubstanceCalculator.DescriptorName;
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
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

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
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