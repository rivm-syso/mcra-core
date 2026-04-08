using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySubstanceSection : InternalExposureDistributionSectionBase<SubstanceContributorKey, ExposureBySubstanceRecord, ExposureBySubstanceBoxPlotRecord> {
        public override string DescriptorKey => ExposureBySubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySubstanceCalculator.DescriptorName;

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregates,
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
            var exposureCollection = ExposureBySubstanceCalculator.CalculateExposures(
                aggregates,
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