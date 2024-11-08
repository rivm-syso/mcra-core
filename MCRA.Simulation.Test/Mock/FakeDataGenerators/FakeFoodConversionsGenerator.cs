using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock food conversions
    /// </summary>
    public static class FakeFoodConversionsGenerator {

        /// <summary>
        /// Creates food conversions based on food translations.
        /// </summary>
        /// <param name="foodTranslations"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static List<FoodConversionResult> Create(
            ICollection<FoodTranslation> foodTranslations,
            ICollection<Compound> substances
        ) {
            var foodConversionResults = new List<FoodConversionResult>();
            foreach (var translation in foodTranslations) {
                foreach (var substance in substances) {
                    var conversionStepResults = new List<FoodConversionResultStep>() {
                        new FoodConversionResultStep() {
                            Finished = false,
                            FoodCodeFrom = translation.FoodFrom.Code,
                            FoodCodeTo = translation.FoodTo.Code,
                            Step = FoodConversionStepType.CompositionExact,
                        },
                        new FoodConversionResultStep() {
                            Finished = true,
                            FoodCodeFrom = translation.FoodTo.Code,
                            FoodCodeTo = translation.FoodTo.Code,
                            Step = FoodConversionStepType.Concentration,
                        }
                    };
                    var foodConversionResult = new FoodConversionResult() {
                        FoodAsEaten = translation.FoodFrom,
                        FoodAsMeasured = translation.FoodTo,
                        Proportion = translation.Proportion,
                        Compound = substance,
                        ConversionStepResults = conversionStepResults
                    };
                    foodConversionResults.Add(foodConversionResult);
                }
            }
            return foodConversionResults;
        }

        /// <summary>
        /// Creates a list of food conversion results based on food as etaen and
        /// modelled food.
        /// </summary>
        /// <param name="foodsAsEaten"></param>
        /// <param name="foodsAsMeasured"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static List<FoodConversionResult> Create(
            List<Food> foodsAsEaten,
            List<Food> foodsAsMeasured,
            List<Compound> substances
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foodConversionResults = new List<FoodConversionResult>();
            foreach (var substance in substances) {
                foreach (var foodAsEaten in foodsAsEaten) {
                    foreach (var foodAsMeasured in foodsAsMeasured) {
                        var conversionStepResults = new List<FoodConversionResultStep>() {
                            new FoodConversionResultStep() {
                                Finished = true,
                                FoodCodeFrom = foodAsEaten.Code,
                                FoodCodeTo = foodAsMeasured.Code,
                                Step = FoodConversionStepType.Concentration,
                            }
                        };
                        var foodConversionResult = new FoodConversionResult() {
                            FoodAsEaten = foodAsEaten,
                            FoodAsMeasured = foodAsMeasured,
                            Proportion = random.NextDouble() * 100,
                            Compound = substance,
                            ConversionStepResults = conversionStepResults
                        };
                        foodConversionResults.Add(foodConversionResult);
                    }
                }
            }
            return foodConversionResults;
        }
    }
}
