using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public static class IndividualSubsetCalculator {
        public static ICollection<Individual> GetIndividualSubsets(
            ICollection<Individual> allIndividuals,
            IDictionary<string, IndividualProperty> allIndividualProperties,
            Population selectedPopulation,
            string surveyCode,
            IndividualSubsetType matchIndividualSubsetWithPopulation,
            List<string> selectedSurveySubsetProperties,
            bool useSamplingWeights
        ) {
            // Get individuals
            var availableIndividuals = allIndividuals
                .Where(r => r.CodeFoodSurvey.Equals(surveyCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var individualFilters = individualsSubsetCalculator.Create(
                selectedPopulation,
                allIndividualProperties,
                matchIndividualSubsetWithPopulation,
                selectedSurveySubsetProperties
            );

            // Get the individuals from individual subset
            var individuals = IndividualsSubsetCalculator
                .ComputeIndividualsSubset(
                    availableIndividuals,
                    individualFilters
                );

            // Overwrite sampling weight
            if (!useSamplingWeights) {
                foreach (var individual in individuals) {
                    individual.SamplingWeight = 1D;
                }
            }

            return individuals;
        }
    }
}
