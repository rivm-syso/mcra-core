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
        /// <param name="individualsSubsetDefinitions"></param>
        /// <param name="individualDaySubsetDefinition"></param>
        /// <returns></returns>
        public static Dictionary<string, PopulationIndividualPropertyValue> Compute(
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions,
            IndividualDaySubsetDefinition individualDaySubsetDefinition
        ) {
            var populationIndividualPropertyValues = CalculateIndividualProperties(individualsSubsetDefinitions);
            //Get explicit population individual day properties for Compute setting
            var individualDayProperties = CalculateIndividualDayProperties(individualDaySubsetDefinition);
            foreach (var item in individualDayProperties) {
                populationIndividualPropertyValues[item.IndividualProperty.Name] = item;
            }
            return populationIndividualPropertyValues;
        }

        /// <summary>
        /// Creates a list of population individual property values matching the
        /// individual subset definitions.
        /// </summary>
        /// <param name="individualsSubsetDefinitions"></param>
        /// <returns></returns>
        private static Dictionary<string, PopulationIndividualPropertyValue> CalculateIndividualProperties(
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions
        ) {
            if (individualsSubsetDefinitions.Count == 0) {
                return [];
            }
            var populationIndividualPropertyValues = individualsSubsetDefinitions.Select(definition => {
                var value = definition.GetQueryDefinitionType() == QueryDefinitionType.ValueList ? definition.IndividualPropertyQuery.Replace("'", string.Empty) : null;
                var populationIndividualPropertyValue = new PopulationIndividualPropertyValue() {
                    MinValue = definition.GetRangeMin(),
                    MaxValue = definition.GetRangeMax(),
                    Value = value,
                    IndividualProperty = new IndividualProperty() {
                        PropertyLevel = PropertyLevelType.Individual,
                        PropertyType = definition.GetQueryDefinitionType() != QueryDefinitionType.ValueList ? IndividualPropertyType.Numeric : IndividualPropertyType.Categorical,
                        CategoricalLevels = new List<string> { value }.ToHashSet(),
                        Name = definition.NameIndividualProperty
                    }
                };
                return populationIndividualPropertyValue;
            }).ToDictionary(c => c.IndividualProperty.Name);

            return populationIndividualPropertyValues;
        }

        /// <summary>
        /// Creates a list of population individual property values matching the
        /// individual day subset definition. Currently only available for Month, 
        /// return null when not available, 
        /// see also FoodSurveySettingsService l 
        /// </summary>
        /// <param name="individualDaySubsetDefinition"></param>
        /// <returns></returns>
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
                        PropertyLevel = PropertyLevelType.IndividualDay,
                        Name = IndividualPropertyType.Month.GetDisplayName(),
                        PropertyType = IndividualPropertyType.Month
                    }
                };
                result.Add(populationIndividualPropertyValue);
            }
            return result;
        }
    }
}
