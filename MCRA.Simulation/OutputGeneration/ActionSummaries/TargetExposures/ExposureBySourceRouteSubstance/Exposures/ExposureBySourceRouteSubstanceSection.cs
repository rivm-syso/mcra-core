using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRouteSubstanceSection : InternalExposureDistributionSectionBase<SourceRouteSubstanceContributorKey, ExposureBySourceRouteSubstanceRecord, ExposureBySourceRouteSubstanceBoxPlotRecord> {
        public override string DescriptorKey => ExposureBySourceRouteSubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySourceRouteSubstanceCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            // The relative potency factors and membership probabilities are not relevant for this summary,
            // so we can pass in dummy values for those parameters.
            var exposureCollection = ExposureBySourceRouteSubstanceCalculator.CalculateExposures(
                externalIndividualExposures,
                activeSubstances,
                activeSubstances.ToDictionary(r => r, r => 1D),
                activeSubstances.ToDictionary(r => r, r => 1D),
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
