using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock single value consumptions.
    /// </summary>
    public static class FakePopulationConsumptionsSingleValuesGenerator {

        /// <summary>
        /// Generates single value consumptions.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="foods"></param>
        /// <param name="valueType"></param>
        /// <param name="percentile"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<PopulationConsumptionSingleValue> Create(
            Population population,
            ICollection<Food> foods,
            IRandom random,
            ConsumptionValueType valueType = ConsumptionValueType.MeanConsumption,
            double percentile = double.NaN,
            double mu = 0,
            double sigma = 1
        ) {
            var result = foods
                .Select(r => new PopulationConsumptionSingleValue() {
                    Population = population,
                    Food = r,
                    ConsumptionAmount = LogNormalDistribution.Draw(random, mu, sigma),
                    ConsumptionUnit = ConsumptionIntakeUnit.gPerDay,
                    ValueType = valueType,
                    Percentile = percentile,
                })
                .ToList();
            return result;
        }
    }
}