using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionByRouteSubstanceTotalSection : InternalExposureContributionSectionBase<RouteSubstanceContributorKey, ContributionByRouteSubstanceRecord> {
        public override string DescriptorKey => ExposureByRouteSubstanceCalculator.DescriptorKey;
        public override string DescriptorName => ExposureByRouteSubstanceCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureByRouteSubstanceCalculator.CalculateExposures(
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
            var exposureCollection = ExposureByRouteSubstanceCalculator.CalculateExposures(
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
