using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.IndividualDaysGenerator {
    public static class HbmIndividualSubsetCalculator {
        public static ICollection<Individual> GetIndividualSubsets(
            ICollection<Individual> allHumanMonitoringIndividuals,
            IDictionary<string, IndividualProperty> allHumanMonitoringIndividualProperties,
            Population selectedPopulation,
            HumanMonitoringSurvey survey,
            IndividualSubsetType matchHbmIndividualSubsetWithPopulation,
            List<string> selectedHbmSurveySubsetProperties,
            bool useHbmSamplingWeights            
        ) {
            // Get individuals
            var availableIndividuals = allHumanMonitoringIndividuals
                .Where(r => r.CodeFoodSurvey.Equals(survey.Code, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var individualFilters = individualsSubsetCalculator.Create(
                selectedPopulation,
                allHumanMonitoringIndividualProperties,
                matchHbmIndividualSubsetWithPopulation,
                selectedHbmSurveySubsetProperties
            );

            // Get the individuals from individual subset
            var individuals = IndividualsSubsetCalculator
                .ComputeIndividualsSubset(
                    availableIndividuals,
                    individualFilters
                );

            // Overwrite sampling weight
            if (!useHbmSamplingWeights) {
                foreach (var individual in individuals) {
                    individual.SamplingWeight = 1D;
                }
            }

            return individuals;
        }
    }
}
