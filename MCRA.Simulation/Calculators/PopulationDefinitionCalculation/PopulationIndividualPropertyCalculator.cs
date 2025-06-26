using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.PopulationDefinitionCalculation {
    public sealed class PopulationIndividualPropertyCalculator {

        /// <summary>
        /// Creates a list of population individual property values matching the
        /// individual and individual-day subset definitions.
        /// </summary>
        public static Dictionary<string, PopulationIndividualPropertyValue> Compute(
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions,
            IndividualDaySubsetDefinition individualDaySubsetDefinition
        ) {
            // Get population individual properties from subset definitions
            var individualPropertyValues = CalculateIndividualProperties(individualsSubsetDefinitions);

            // Get population individual-day properties from subset definitions
            var individualDayPropertyValues = CalculateIndividualDayProperties(individualDaySubsetDefinition);

            var result = individualPropertyValues.Values
                .Union(individualDayPropertyValues)
                .ToDictionary(r => r.IndividualProperty.Code, StringComparer.OrdinalIgnoreCase);
            return result;
        }

        /// <summary>
        /// Creates a list of population individual property values matching the
        /// individual subset definitions.
        /// </summary>
        public static Dictionary<string, PopulationIndividualPropertyValue> CalculateIndividualProperties(
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions
        ) {
            return individualsSubsetDefinitions?
                .Select(CreatePopulationIndividualPropertyValue)
                .ToDictionary(r => r.IndividualProperty.Code, StringComparer.OrdinalIgnoreCase) 
                ?? [];
        }

        /// <summary>
        /// Creates a list of population individual property values matching the
        /// individual day subset definition. Currently only available for Month,
        /// return empty list when not available.
        /// </summary>
        private static List<PopulationIndividualPropertyValue> CalculateIndividualDayProperties(
            IndividualDaySubsetDefinition individualDaySubsetDefinition
        ) {
            var result = new List<PopulationIndividualPropertyValue>();
            if (individualDaySubsetDefinition?.MonthsSubset?.Count > 0) {
                var months = individualDaySubsetDefinition
                   .MonthsSubset
                   .Select(c => Enum.TryParse(c.ToString(), out MonthType month) ? month.ToString() : "Unknown")
                   .ToList();
                var populationIndividualPropertyValue = new PopulationIndividualPropertyValue() {
                    Value = string.Join(", ", months),
                    IndividualProperty = new IndividualProperty() {
                        Code = IndividualPropertyType.Month.GetDisplayName(),
                        Name = IndividualPropertyType.Month.GetDisplayName(),
                        PropertyLevel = PropertyLevelType.IndividualDay,
                        PropertyType = IndividualPropertyType.Month
                    }
                };
                result.Add(populationIndividualPropertyValue);
            }
            return result;
        }

        public static PopulationIndividualPropertyValue CreatePopulationIndividualPropertyValue(
            IndividualsSubsetDefinition definition
        ) {
            var queryDefinitionType = definition.GetQueryDefinitionType();
            var individualProperty = new IndividualProperty() {
                Code = definition.NameIndividualProperty,
                Name = definition.NameIndividualProperty,
                PropertyLevel = PropertyLevelType.Individual,
                PropertyType = queryDefinitionType != QueryDefinitionType.ValueList
                    ? IndividualPropertyType.Numeric
                    : IndividualPropertyType.Categorical,
                CategoricalLevels = queryDefinitionType == QueryDefinitionType.ValueList
                    ? definition.GetQueryKeywords() : null
            };
            var result = new PopulationIndividualPropertyValue() {
                MinValue = definition.GetRangeMin(),
                MaxValue = definition.GetRangeMax(),
                Value = definition.IndividualPropertyQuery?.Replace("'", string.Empty),
                IndividualProperty = individualProperty
            };
            return result;
        }
    }
}
