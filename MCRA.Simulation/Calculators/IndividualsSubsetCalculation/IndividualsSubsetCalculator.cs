using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.IndividualFilters;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public sealed class IndividualsSubsetCalculator {

        /// <summary>
        /// Gets the individuals in the selected subset.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="individualSubsetFilters"></param>
        /// <returns></returns>
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
