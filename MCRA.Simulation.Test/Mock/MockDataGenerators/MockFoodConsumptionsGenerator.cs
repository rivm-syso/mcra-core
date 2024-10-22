using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock food consumptions
    /// </summary>
    public static class MockFoodConsumptionsGenerator {

        /// <summary>
        /// Creates a list of food consumptions using the provided
        /// consumption generation function.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDays"></param>
        /// <param name="numNonConsumers"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<FoodConsumption> Create(
            ICollection<Food> foods,
            ICollection<IndividualDay> individualDays,
            IRandom random,
            int numNonConsumers = 0
        ) {
            return Create(foods, individualDays, numNonConsumers, random, (f, i, r) => createFoodConsumptions(f, i, r));
        }

        /// <summary>
        /// Creates a collection of food consumptions based on the provided
        /// consumption patterns.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDays"></param>
        /// <param name="consumptionPatterns"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<FoodConsumption> Create(
            ICollection<Food> foods,
            List<IndividualDay> individualDays,
            int[,] consumptionPatterns,
            IRandom random
        ) {
            var result = new List<FoodConsumption>();
            for (int i = 0; i < individualDays.Count; i++) {
                for (int j = 0; j < foods.Count; j++) {
                    if (consumptionPatterns[i, j] > 0) {
                        var foodConsumption = new FoodConsumption() {
                            IndividualDay = individualDays[i],
                            Food = foods.ElementAt(j),
                            Amount = consumptionPatterns[i, j] * random.NextDouble() * 100,
                        };
                        result.Add(foodConsumption);
                    };
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a list of food consumptions.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDay"></param>
        /// <param name="numNonConsumers">
        /// <param name="random"></param>
        /// <param name="consumptionsGenerator"></param>
        /// Number of individuals not consuming anything.
        /// Skips the first n individuals.
        /// </param>
        /// <returns></returns>
        public static List<FoodConsumption> Create(
            ICollection<Food> foods,
            ICollection<IndividualDay> individualDay,
            int numNonConsumers,
            IRandom random,
            Func<ICollection<Food>, IndividualDay, IRandom, List<FoodConsumption>> consumptionsGenerator
        ) {
            var foodConsumptions = new List<FoodConsumption>();
            var individuals = individualDay
                .GroupBy(r => r.Individual)
                .Skip(numNonConsumers);
            foreach (var individual in individuals) {
                foreach (var day in individual) {
                    foodConsumptions.AddRange(consumptionsGenerator(foods, day, random));
                }
            }
            return foodConsumptions;
        }

        /// <summary>
        /// Gets the consumption
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDay"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<FoodConsumption> createFoodConsumptions(
            ICollection<Food> foods,
            IndividualDay individualDay,
            IRandom random
        ) {
            var result = new List<FoodConsumption>();
            foreach (var food in foods) {
                if (random.Next(2) != 0) {
                    var foodConsumption = new FoodConsumption() {
                        IndividualDay = individualDay,
                        idMeal = "meal",
                        Food = food,
                        Amount = random.NextDouble() * 100,
                        DateConsumed = new DateTime(),
                    };
                    result.Add(foodConsumption);
                };
            }
            return result;
        }
    }
}
