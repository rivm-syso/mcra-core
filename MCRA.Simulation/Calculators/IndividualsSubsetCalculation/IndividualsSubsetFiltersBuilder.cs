using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Filters.IndividualFilters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IndividualsSubsetCalculation {
    public sealed class IndividualsSubsetFiltersBuilder {

        /// <summary>
        /// Creates a collection of individual filters from the property ranges
        /// specified by the population.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="individualProperties"></param>
        /// <param name="individualSubsetType"></param>
        /// <param name="selectedFilterProperties"></param>
        /// <returns></returns>
        public ICollection<IPropertyIndividualFilter> Create(
            Population population,
            IDictionary<string, IndividualProperty> individualProperties,
            IndividualSubsetType individualSubsetType,
            ICollection<string> selectedFilterProperties
        ) {
            if (individualSubsetType == IndividualSubsetType.IgnorePopulationDefinition
                || individualProperties == null
            ) {
                return null;
            }
            var result = new List<IPropertyIndividualFilter>();

            var surveyIndividualProperties = individualProperties.Values;
            var matchSelectedProperties = individualSubsetType == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties;
            foreach (var individualProperty in population.PopulationIndividualPropertyValues) {
                var property = individualProperty.Value.IndividualProperty;
                if (property.PropertyLevel != PropertyLevelType.Individual) {
                    continue;
                }
                if (matchSelectedProperties && !selectedFilterProperties.Contains(property.Name)) {
                    continue;
                }

                // TODO: Matching is now based on identical codes.
                // It should also include controlled terminology aliases
                var matchedProperty = surveyIndividualProperties
                    .FirstOrDefault(r => string.Equals(individualProperty.Key, r.Code, StringComparison.OrdinalIgnoreCase));

                if (matchedProperty != null) {
                    IPropertyIndividualFilter filter = null;
                    if (property.PropertyType == IndividualPropertyType.Gender) {
                        filter = new GenderPropertyIndividualFilter(matchedProperty, individualProperty.Value.CategoricalLevels);
                    } else if (property.PropertyType == IndividualPropertyType.Boolean) {
                        filter = new BooleanPropertyIndividualFilter(matchedProperty, individualProperty.Value.CategoricalLevels);
                    } else if (property.PropertyType == IndividualPropertyType.Numeric
                        || property.PropertyType == IndividualPropertyType.Integer
                        || property.PropertyType == IndividualPropertyType.Nonnegative
                        || property.PropertyType == IndividualPropertyType.NonnegativeInteger
                    ) {
                        filter = new NumericPropertyIndividualFilter(matchedProperty, individualProperty.Value.MinValue, individualProperty.Value.MaxValue);
                    } else if (property.PropertyType == IndividualPropertyType.Location) {
                        // Skip location; should be filtered by survey.
                        continue;
                    } else if (property.PropertyType == IndividualPropertyType.Categorical) {
                        if (!individualProperty.Value?.CategoricalLevels?.Any() ?? true) {
                            // No levels; skip this filter
                            continue;
                        }
                        filter = new CategoricalPropertyIndividualFilter(matchedProperty, individualProperty.Value.CategoricalLevels);
                    } else if (property.PropertyType == IndividualPropertyType.DateTime) {
                        // TODO
                        throw new NotImplementedException($"Individual filter for property of type {property.PropertyType} not implemented.");
                    } else if (property.PropertyType == IndividualPropertyType.Month) {
                        // TODO
                        throw new NotImplementedException($"Individual filter for property of type {property.PropertyType} not implemented.");
                    } else {
                        throw new NotImplementedException($"Individual filter for property of type {property.PropertyType} not implemented.");
                    }

                    result.Add(filter);
                }
            }
            return result;
        }
    }
}
