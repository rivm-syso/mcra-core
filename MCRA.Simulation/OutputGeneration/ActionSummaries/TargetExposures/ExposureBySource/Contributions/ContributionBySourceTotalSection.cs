using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceTotalSection : ContributionBySourceSectionBase {

        public void Summarize(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var observedIndividualMeans = GetInternalObservedIndividualMeans(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            Records = SummarizeContributions(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                routes,
                kineticConversionFactors,
                externalExposureUnit,
                null,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson
            );
        }

        public void SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var observedIndividualMeans = GetInternalObservedIndividualMeans(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            var records = SummarizeUncertainty(
                 externalExposureCollections,
                 observedIndividualMeans,
                 relativePotencyFactors,
                 membershipProbabilities,
                 routes,
                 kineticConversionFactors,
                 externalExposureUnit,
                 null,
                 isPerPerson
             );
            UpdateContributions(records);
        }
    }
}
