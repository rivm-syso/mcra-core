using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock food translations
    /// </summary>
    public static class MockFoodTranslationsGenerator {

        /// <summary>
        /// Creates food translations
        /// The interpretation of a foodtranslation is: foodFrom is foodAsEaten, FoodTo is modelled food
        /// These proportions are smaller than 100%.
        /// Additional, each modelled food can be eaten with a proportion of 100%
        /// </summary>
        /// <param name="allFoods"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<FoodTranslation> Create(
            ICollection<Food> allFoods,
            IRandom random
        ) {
            var count = allFoods.Count / 2;
            if (count == 0) {
                throw new Exception("Suply 2 foods or more to get sensible translations");
            }
            var foodsAsEaten = allFoods.Take(count);
            var foodsAsMeasured = allFoods.Skip(count);
            var foodTranslations = new List<FoodTranslation>();
            foreach (var foodFrom in foodsAsEaten) {
                var translations = new List<FoodTranslation>();
                foreach (var foodTo in foodsAsMeasured) {
                    var proportion = random.NextDouble() * 100;
                    var foodTranslation = new FoodTranslation() {
                        FoodFrom = foodFrom,
                        FoodTo = foodTo,
                        Proportion = proportion,
                        IdPopulation = "population",
                    };
                    translations.Add(foodTranslation);
                    foodTranslations.Add(foodTranslation);
                }
            }
            foreach (var foodTo in foodsAsMeasured) {
                var foodTranslation = new FoodTranslation() {
                    FoodFrom = foodTo,
                    FoodTo = foodTo,
                    Proportion = 100,
                    IdPopulation = "population",
                };
                foodTranslations.Add(foodTranslation);
            }
            return foodTranslations;
        }
    }
}
