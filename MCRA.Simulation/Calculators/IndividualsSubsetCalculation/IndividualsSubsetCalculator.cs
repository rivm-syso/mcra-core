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
                        .Where(ind => filter.Passes(ind))
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
                individualProperties.TryGetValue(nameCofactor, out var cofactor);
                foreach (var item in individualSubset) {
                    var cofactorResult = cofactor != null
                        ? item.IndividualPropertyValues.FirstOrDefault(ip => ip.IndividualProperty == cofactor)
                        : null;
                    item.Cofactor = cofactorResult?.Value;
                }
            }

            if (!string.IsNullOrEmpty(nameCovariable)) {
                individualProperties.TryGetValue(nameCovariable, out var covariable);
                foreach (var item in individualSubset) {
                    var covariableResult = covariable != null
                        ? item.IndividualPropertyValues.FirstOrDefault(ip => ip.IndividualProperty == covariable)
                        : null;
                    item.Covariable = covariableResult?.DoubleValue ?? double.NaN;
                }
            }
        }
    }
}
