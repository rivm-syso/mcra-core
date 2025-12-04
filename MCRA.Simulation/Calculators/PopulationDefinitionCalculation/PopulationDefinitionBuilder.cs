using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Objects.Populations;

namespace MCRA.Simulation.Calculators.PopulationDefinitionCalculation {
    public class PopulationDefinitionBuilder {

        /// <summary>
        /// Creates a population from specified settings and subset-definitions.
        /// </summary>
        public SimulatedPopulation Create(
            double nominalBodyWeight,
            bool definitionFromSubsetSettings,
            List<IndividualsSubsetDefinition> individualsSubsetDefinitions,
            IndividualDaySubsetDefinition individualDaySubsetDefinition
        ) {
            var population = new Population() {
                Code = "Generated",
                NominalBodyWeight = nominalBodyWeight
            };

            // Get explicit population individual properties for compute setting
            if (definitionFromSubsetSettings) {
                var populationIndividualPropertyValues = PopulationIndividualPropertyCalculator
                    .Compute(individualsSubsetDefinitions, individualDaySubsetDefinition);
                population.PopulationIndividualPropertyValues = populationIndividualPropertyValues;
            } else {
                population.PopulationIndividualPropertyValues = [];
            }
            var result = new SimulatedPopulation(population);
            return result;
        }
    }
}
