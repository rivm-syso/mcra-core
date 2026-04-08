using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySubstancePercentilesSection : InternalExposurePercentileSectionBase<SubstanceContributorKey, ExposureBySubstancePercentileRecord> {
        public override string DescriptorKey => ExposureBySubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySubstanceCalculator.DescriptorName;

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureBySubstanceCalculator.CalculateExposures(
                aggregates,
                activeSubstances,
                activeSubstances.ToDictionary(r => r, r => 1D),
                activeSubstances.ToDictionary(r => r, r => 1D),
                kineticConversionFactors,
                isPerPerson
            );
            summarize(exposureCollection, uncertaintyLowerBound, uncertaintyUpperBound, outputStratifier, percentages);
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregates,
            ICollection<Compound> activeSubstances,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            List<double> percentages,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureBySubstanceCalculator.CalculateExposures(
                aggregates,
                activeSubstances,
                activeSubstances.ToDictionary(r => r, r => 1D),
                activeSubstances.ToDictionary(r => r, r => 1D),
                kineticConversionFactors,
                isPerPerson
            );
            summarize(percentages, outputStratifier, exposureCollection);
        }
    }
}
