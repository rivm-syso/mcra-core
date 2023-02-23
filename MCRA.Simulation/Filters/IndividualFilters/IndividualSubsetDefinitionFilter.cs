using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Filters.IndividualFilters {

    public class IndividualSubsetDefinitionFilter : IPropertyIndividualFilter {

        private readonly IndividualsSubsetDefinitionDto _individualSubsetDefinition;

        public IndividualProperty IndividualProperty { get; private set; }

        public bool IncludeMissingValueRecords { get; set; }

        public IndividualSubsetDefinitionFilter(
            IndividualProperty individualProperty,
            IndividualsSubsetDefinitionDto individualSubsetDefinition
        ) {
            IndividualProperty = individualProperty;
            _individualSubsetDefinition = individualSubsetDefinition;
        }

        /// <summary>
        /// Returns true if the individual passes the filter. I.e., if the property value
        /// of the property of this filter is accepted by the subset definition.
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        public bool Passes(Individual individual) {
            var propertyValue = individual.IndividualPropertyValues
                .FirstOrDefault(r => r.IndividualProperty == IndividualProperty);
            return matches(propertyValue);
        }

        /// <summary>
        /// Evaluates whether the given individual property matches the subset definition.
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private bool matches(IndividualPropertyValue propertyValue) {
            var queryDefinitionType = _individualSubsetDefinition.GetQueryDefinitionType();
            if (queryDefinitionType == QueryDefinitionType.Empty) {
                return true;
            } else if (queryDefinitionType == QueryDefinitionType.ValueList) {
                if (propertyValue == null) {
                    return false;
                }
                var keywords = _individualSubsetDefinition.GetQueryKeywords();
                return keywords?.Contains(propertyValue.Value) ?? false;
            } else if (queryDefinitionType == QueryDefinitionType.Range) {
                if (propertyValue == null) {
                    return false;
                }

                // Smaller/equal parameter
                var smallerThanString = _individualSubsetDefinition.IndividualPropertyQuery.GetSmallerEqualString();
                if (smallerThanString != string.Empty) {
                    var absmin = double.Parse(_individualSubsetDefinition.IndividualPropertyQuery.GetSmallerEqualString().Replace("-", string.Empty));
                    return propertyValue.DoubleValue <= absmin;
                }

                // Range parameters
                foreach (var strRange in _individualSubsetDefinition.IndividualPropertyQuery.GetRangeStrings()) {
                    var min = double.Parse(strRange.Split('-')[0]);
                    var max = double.Parse(strRange.Split('-')[1]);
                    return propertyValue.DoubleValue >= min && propertyValue.DoubleValue <= max;
                }

                // Greater/equal parameter
                var greaterThanString = _individualSubsetDefinition.IndividualPropertyQuery.GetGreaterEqualString();
                if (greaterThanString != string.Empty) {
                    var absmax = double.Parse(_individualSubsetDefinition.IndividualPropertyQuery.GetGreaterEqualString().Replace("-", string.Empty));
                    return propertyValue.DoubleValue >= absmax;
                }

                return false;
            }
            return true;
        }
    }
}
