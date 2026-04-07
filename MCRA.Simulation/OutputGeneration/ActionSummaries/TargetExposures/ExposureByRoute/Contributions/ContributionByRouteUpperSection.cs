using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionByRouteUpperSection : InternalExposureUpperContributionSectionBase<RouteContributorKey, ContributionByRouteRecord> {

        public override string DescriptorKey => ExposureByRouteCalculator.DescriptorKey;
        public override string DescriptorName => ExposureByRouteCalculator.DescriptorName;

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
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
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
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
