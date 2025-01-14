using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on categorical individual property values.
    /// </summary>
    public class CategoricalPropertyIndividualFilter : PropertyIndividualFilterBase, IFilter<Individual> {

        public HashSet<string> AcceptedValues { get; protected set; }

        public CategoricalPropertyIndividualFilter(
            IndividualProperty individualProperty,
            ICollection<string> subsetValues
        ) : base(individualProperty) {
            IndividualProperty = individualProperty;
            AcceptedValues = subsetValues.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if the individual passes the filter. I.e., if the property value
        /// of the property of this filter is accepted by the subset definition.
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        public override bool Passes(Individual individual) {
            var propertyValue = individual.GetPropertyValue(IndividualProperty);
            return matches(propertyValue);
        }

        /// <summary>
        /// Evaluates whether the given individual property matches the subset definition.
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private bool matches(IndividualPropertyValue propertyValue) {
            if (string.IsNullOrEmpty(propertyValue?.TextValue)) {
                return IncludeMissingValueRecords;
            }
            return AcceptedValues.Contains(propertyValue.Value);
        }
    }
}
