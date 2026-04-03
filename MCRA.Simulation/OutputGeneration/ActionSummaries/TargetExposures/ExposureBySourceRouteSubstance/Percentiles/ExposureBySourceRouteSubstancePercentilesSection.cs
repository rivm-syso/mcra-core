using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRouteSubstancePercentilesSection : InternalExposurePercentileSectionBase<SourceRouteSubstanceContributorKey, ExposureBySourceRouteSubstancePercentileRecord> {

        public override string DescriptorKey => ExposureBySourceRouteSubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureBySourceRouteSubstanceCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            var exposureCollection = ExposureBySourceRouteSubstanceCalculator.CalculateExposures(
                externalIndividualExposures,
                activeSubstances,
                activeSubstances.ToDictionary(r => r, r => 1D),
                activeSubstances.ToDictionary(r => r, r => 1D),
                kineticConversionFactors,
                isPerPerson
            );
            summarize(exposureCollection, uncertaintyLowerBound, uncertaintyUpperBound, outputStratifier, percentages);
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            var exposureCollection = ExposureBySourceRouteSubstanceCalculator.CalculateExposures(
                externalIndividualExposures,
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
