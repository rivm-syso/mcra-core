using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualFilters {

    /// <summary>
    /// Filter for subsetting individuals based on gender individual property values.
    /// </summary>
    public class NumericPropertyIndividualFilter : PropertyIndividualFilterBase, IFilter<Individual> {

        public double? Min { get; private set; }
        public double? Max { get; private set; }

        public NumericPropertyIndividualFilter(
            IndividualProperty individualProperty,
            double? min,
            double? max
        ) : base(individualProperty) {
            IndividualProperty = individualProperty;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Returns true if the individual passes the filter. I.e., if the property value
        /// of the property of this filter is accepted by the subset definition.
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        public override bool Passes(Individual individual) {
            var propertyValue = individual.IndividualPropertyValues
                .FirstOrDefault(r => r.IndividualProperty == IndividualProperty);
            if (propertyValue == null) {
                return IncludeMissingValueRecords;
            }
            return matches(propertyValue);
        }

        /// <summary>
        /// Evaluates whether the given individual property matches the subset definition.
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private bool matches(IndividualPropertyValue propertyValue) {
            if (!propertyValue.DoubleValue.HasValue) {
                return IncludeMissingValueRecords;
            }
            if (Min.HasValue && propertyValue.DoubleValue.Value < Min.Value) {
                return false;
            }
            if (Max.HasValue && propertyValue.DoubleValue.Value > Max.Value) {
                return false;
            }
            return true;
        }
    }
}
