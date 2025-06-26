using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.PopulationDefinitionCalculation {
    public class PopulationDefinitionBuilder {

        /// <summary>
        /// Creates a population from specified settings and subset-definitions.
        /// </summary>
        public Population Create(
            double nominalBodyWeight,
            bool definitionFromSubsetSettings,
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions,
            IndividualDaySubsetDefinition individualDaySubsetDefinition
        ) {
            var result = new Population() {
                Code = "Generated",
                NominalBodyWeight = nominalBodyWeight
            };

            // Get explicit population individual properties for compute setting
            if (definitionFromSubsetSettings) {
                var populationIndividualPropertyValues = PopulationIndividualPropertyCalculator
                    .Compute(individualsSubsetDefinitions, individualDaySubsetDefinition);
                result.PopulationIndividualPropertyValues = populationIndividualPropertyValues;
            } else {
                result.PopulationIndividualPropertyValues = [];
            }
            return result;
        }
    }
}
