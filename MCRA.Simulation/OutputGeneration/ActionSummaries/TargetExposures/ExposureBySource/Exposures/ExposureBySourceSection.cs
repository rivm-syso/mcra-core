using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceSection : InternalExposureDistributionSectionBase<SourceContributorKey, ExposureBySourceRecord, ExposureBySourceBoxPlotRecord> {

        public override string DescriptorKey => ExposureBySourceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySourceCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool isPerPerson,
            PopulationStratifier outputStratifier,
            bool skipPrivacySensitiveOutputs
        ) {
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            summarize(
                exposureCollection,
                targetUnit,
                outputStratifier,
                skipPrivacySensitiveOutputs,
                lowerPercentage,
                upperPercentage
            );
        }
    }
}
