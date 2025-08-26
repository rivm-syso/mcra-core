using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public sealed class IndividualsSubsetFiltersBuilder {

        /// <summary>
        /// Creates a collection of individual filters from the property ranges
        /// specified by the population.
        /// </summary>
        public ICollection<IPropertyIndividualFilter> Create(
            Population population,
            ICollection<IndividualProperty> surveyIndividualProperties,
            IndividualSubsetType individualSubsetType,
            ICollection<string> selectedFilterProperties = null
        ) {
            return Create(
                population?.PopulationIndividualPropertyValues?.Values,
                surveyIndividualProperties,
                individualSubsetType,
                selectedFilterProperties
            );
        }

        /// <summary>
        /// Creates a collection of individual filters from the property ranges
        /// specified by the population.
        /// </summary>
        public ICollection<IPropertyIndividualFilter> Create(
            ICollection<PopulationIndividualPropertyValue> populationIndividualProperties,
            ICollection<IndividualProperty> surveyIndividualProperties,
            IndividualSubsetType individualSubsetType,
            ICollection<string> selectedFilterProperties = null
        ) {
            if (individualSubsetType == IndividualSubsetType.IgnorePopulationDefinition
                || surveyIndividualProperties == null
                || populationIndividualProperties == null
            ) {
                return null;
            }
            var result = new List<IPropertyIndividualFilter>();

            var matchSelectedProperties = individualSubsetType == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties;
            foreach (var propertyValue in populationIndividualProperties) {
                var property = propertyValue.IndividualProperty;
                if (property.PropertyLevel != PropertyLevelType.Individual) {
                    continue;
                }
                if (matchSelectedProperties && !selectedFilterProperties.Contains(property.Name)) {
                    continue;
                }

                var matchedProperty = surveyIndividualProperties
                    .FirstOrDefault(r => r.MatchesIndividualProperty(propertyValue.IndividualProperty));
                if (matchedProperty != null) {
                    var filter = Create(
                        property,
                        propertyValue.CategoricalLevels,
                        propertyValue.MinValue,
                        propertyValue.MaxValue
                    );
                    if (filter != null) {
                        result.Add(filter);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a property individual filter for the specified individual property
        /// based on the provided categorical levels and min- and max value.
        /// </summary>
        public static IPropertyIndividualFilter Create(
            IndividualProperty property,
            HashSet<string> categoricalLevels,
            double? minValue,
            double? maxValue
        ) {
            if (property.IsSexProperty) {
                return new GenderPropertyIndividualFilter(
                    property,
                    categoricalLevels
                );
            } else if (property.PropertyType == IndividualPropertyType.Boolean) {
                return new BooleanPropertyIndividualFilter(
                    property,
                    categoricalLevels
                );
            } else if (property.PropertyType == IndividualPropertyType.Numeric
                || property.PropertyType == IndividualPropertyType.Integer
                || property.PropertyType == IndividualPropertyType.Nonnegative
                || property.PropertyType == IndividualPropertyType.NonnegativeInteger
            ) {
                return new NumericPropertyIndividualFilter(
                    property,
                    minValue,
                    maxValue
                );
            } else if (property.PropertyType == IndividualPropertyType.Location) {
                // Skip location; should be filtered by survey.
                return null;
            } else if (property.PropertyType == IndividualPropertyType.Categorical) {
                if (categoricalLevels?.Count > 0) {
                    return new CategoricalPropertyIndividualFilter(
                        property,
                        categoricalLevels
                    );
                } else {
                    // No levels; skip this filter
                    return null;
                }
            } else {
                throw new NotImplementedException($"Individual filter for property of type {property.PropertyType} not implemented.");
            }
        }
    }
}
