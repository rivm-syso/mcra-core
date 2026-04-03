using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceTotalSection : InternalExposureContributionSectionBase<SourceContributorKey, ContributionBySourceRecord> {

        public override string DescriptorKey => ExposureBySourceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySourceCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            PopulationStratifier outputStratifier,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
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

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
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
