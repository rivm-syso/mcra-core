using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceUpperSection : InternalExposureContributionSectionBase<SourceContributorKey, ContributionBySourceRecord> {
        public override string DescriptorKey => "Source";
        public override string DescriptorName => "source";
        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
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

            var upperExposureCollection = getUpperTailExposures(
                exposureCollection,
                percentageForUpperTail,
                true
            );

            Records = summarize(
                upperExposureCollection,
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
            double percentageForUpperTail,
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

            var upperExposureCollection = getUpperTailExposures(
                exposureCollection,
                percentageForUpperTail
            );

            var records = summarizeUncertainty(upperExposureCollection, outputStratifier);
            updateContributions(Records, records);
        }
    }
}