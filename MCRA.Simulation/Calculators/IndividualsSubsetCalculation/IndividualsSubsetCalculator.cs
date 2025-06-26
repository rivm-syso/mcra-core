using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public sealed class IndividualsSubsetCalculator {

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
            var individualFilters = individualsSubsetCalculator
                .Create(
                    selectedPopulation,
                    allIndividualProperties?.Values,
                    matchIndividualSubsetWithPopulation,
                    selectedSurveySubsetProperties
                );

            // Get the individuals from individual subset
            var individuals = ComputeIndividualsSubset(
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

        /// <summary>
        /// Gets the individuals in the selected subset.
        /// </summary>
        public static ICollection<Individual> ComputeIndividualsSubset(
            ICollection<Individual> individuals,
            ICollection<IPropertyIndividualFilter> individualSubsetFilters
        ) {
            if (individuals == null) {
                return null;
            }
            var result = individuals;
            var subsets = new List<IEnumerable<Individual>>();
            if (individualSubsetFilters != null) {
                foreach (var filter in individualSubsetFilters) {
                    result = result
                        .Where(filter.Passes)
                        .ToList();
                }
            }
            return [.. result];
        }

        public static void FillIndividualCofactorCovariableValues(
            IDictionary<string, IndividualProperty> individualProperties,
            string nameCofactor,
            string nameCovariable,
            ICollection<Individual> individualSubset
        ) {
            if (!string.IsNullOrEmpty(nameCofactor)) {
                var hasCofactor = individualProperties.TryGetValue(nameCofactor, out var cofactor);
                foreach (var item in individualSubset) {
                    item.Cofactor = null;
                    if(hasCofactor){
                        item.Cofactor = item.GetPropertyValue(cofactor)?.Value;
                    }
                }
            }

            if (!string.IsNullOrEmpty(nameCovariable)) {
                var hasCovariable = individualProperties.TryGetValue(nameCovariable, out var covariable);
                foreach (var item in individualSubset) {
                    item.Covariable = double.NaN;
                    if (hasCovariable) {
                        item.Covariable = item.GetDoubleValue(covariable) ?? double.NaN;
                    }
                }
            }
        }
    }
}
