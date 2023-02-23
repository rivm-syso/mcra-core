using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock consumptions per modelled food
    /// </summary>
    public static class MockConsumptionsByModelledFoodGenerator {

        /// <summary>
        /// Not very realistic, better to take the other one
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDays"></param>
        /// <param name="isBrand"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<ConsumptionsByModelledFood> Create(
            ICollection<Food> foods,
            ICollection<IndividualDay> individualDays,
            bool isBrand = false,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = new List<ConsumptionsByModelledFood>();
            foreach (var day in individualDays) {
                var consumptions = createConsumptionsPerModelledFood(foods, day, 4, null, isBrand, random.Next());
                result.AddRange(consumptions);
            }
            return result;
        }

        /// <summary>
        /// Gets the consumption
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="individualDay"></param>
        /// <param name="i"></param>
        /// <param name="processingProportions"></param>
        /// <param name="isBrand"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private static List<ConsumptionsByModelledFood> createConsumptionsPerModelledFood(
            ICollection<Food> foods,
            IndividualDay individualDay,
            int i,
            IDictionary<(Food, string), double> processingProportions = null,
            bool isBrand = false,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var consumptionsByModelledFood = new List<ConsumptionsByModelledFood>();
            processingProportions = processingProportions ?? MockFoodsGenerator.CreateReverseYieldFactors(foods, random);
            foreach (var food in foods) {
                var rawFood = food.BaseFood ?? food;
                if (random.Next(0, 2 * i) < i) {
                    var baseFood = food?.BaseFood ?? food;
                    var amount = random.NextDouble() * 100;
                    var proportionProcessing = (food.ProcessingTypes?.Any() ?? false)
                        ? processingProportions[(baseFood, food.ProcessingFacetCode())]
                        : 1D;
                    var consumptionByModelledFood = new ConsumptionsByModelledFood() {
                        FoodAsMeasured = baseFood,
                        ProportionProcessing = proportionProcessing,
                        AmountFoodAsMeasured = amount * random.NextDouble(),
                        ProcessingTypes = food.ProcessingTypes?.ToList(),
                        ConversionResultsPerCompound = new Dictionary<Compound, FoodConversionResult>(),
                        IndividualDay = individualDay,
                        FoodConsumption = new FoodConsumption() {
                            Amount = amount,
                            Food = food,
                            IndividualDay = individualDay
                        },
                        IsBrand = isBrand,
                    };
                    if (isBrand) {
                        food.MarketShare = new MarketShare() { Percentage = 10 };
                    }
                    consumptionByModelledFood.ConversionResultsPerCompound[new Compound()] = new FoodConversionResult() {
                        FoodAsEaten = food,
                        FoodAsMeasured = food
                    };
                    consumptionsByModelledFood.Add(consumptionByModelledFood);
                }
            }
            return consumptionsByModelledFood;
        }

        /// <summary>
        /// Creates realistic values based on consumptions and translations
        /// </summary>
        /// <param name="foodConsumptions"></param>
        /// <param name="foodTranslations"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static ICollection<ConsumptionsByModelledFood> Create(
            ICollection<FoodConsumption> foodConsumptions,
            ICollection<FoodTranslation> foodTranslations,
            ICollection<Compound> substances
        ) {
            var result = new List<ConsumptionsByModelledFood>();
            var groupedConsumptions = foodConsumptions
                .GroupBy(fc => new { fc.Individual, fc.idDay })
                .ToList();

            foreach (var consumptions in groupedConsumptions) {
                var consumptionsByModelledFood = new List<ConsumptionsByModelledFood>();
                foreach (var consumption in consumptions) {

                    var translations = foodTranslations.Where(c => c.FoodFrom == consumption.Food).Select(c => c).ToList();
                    foreach (var translation in translations) {
                        var consumptionPerFoodAsMeasured = new ConsumptionsByModelledFood() {
                            FoodAsMeasured = translation.FoodTo,
                            AmountFoodAsMeasured = consumption.Amount * translation.Proportion / 100,
                            ConversionResultsPerCompound = new Dictionary<Compound, FoodConversionResult>(),
                            IndividualDay = consumption.IndividualDay,
                            FoodConsumption = consumption,
                            IsBrand = false,
                        };
                        foreach (var substance in substances) {
                            consumptionPerFoodAsMeasured.ConversionResultsPerCompound[substance] = new FoodConversionResult() {
                                FoodAsEaten = consumption.Food,
                                FoodAsMeasured = translation.FoodTo,
                                Compound = substance,
                                ConversionStepResults = new List<FoodConversionResultStep>() {
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
                                }
                            };
                        }
                        consumptionsByModelledFood.Add(consumptionPerFoodAsMeasured);
                    }
                }

                if (consumptionsByModelledFood.Count > 0) {
                    result.AddRange(consumptionsByModelledFood);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates realistic values based on consumptions and translations
        /// </summary>
        /// <param name="foodConsumptions"></param>
        /// <param name="foodTranslations"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static ICollection<ConsumptionsByModelledFood> CreateSubstanceDependentConverionPaths(
            ICollection<FoodConsumption> foodConsumptions,
            ICollection<FoodTranslation> foodTranslations,
            List<Compound> substances
        ) {
            var result = new List<ConsumptionsByModelledFood>();
            var groupedConsumptions = foodConsumptions
                .GroupBy(fc => new { fc.Individual, fc.idDay })
                .ToList();

            foreach (var consumptions in groupedConsumptions) {
                var consumptionsByModelledFood = new List<ConsumptionsByModelledFood>();
                foreach (var consumption in consumptions) {

                    var translations = foodTranslations.Where(c => c.FoodFrom == consumption.Food).Select(c => c).ToList();
                    var counter = 0;
                    foreach (var translation in translations) {
                        var consumptionPerFoodAsMeasured = new ConsumptionsByModelledFood() {
                            FoodAsMeasured = translation.FoodTo,
                            AmountFoodAsMeasured = consumption.Amount * translation.Proportion / 100,
                            ConversionResultsPerCompound = new Dictionary<Compound, FoodConversionResult>(),
                            IndividualDay = consumption.IndividualDay,
                            FoodConsumption = consumption,
                            IsBrand = false,
                        };
                        consumptionPerFoodAsMeasured.ConversionResultsPerCompound[substances[counter]] = new FoodConversionResult() {
                            FoodAsEaten = consumption.Food,
                            FoodAsMeasured = translation.FoodTo,
                            Compound = substances[counter],
                            ConversionStepResults = new List<FoodConversionResultStep>() {
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
                                }
                        };
                        consumptionsByModelledFood.Add(consumptionPerFoodAsMeasured);
                        counter++;
                    }
                }

                if (consumptionsByModelledFood.Count > 0) {
                    result.AddRange(consumptionsByModelledFood);
                }
            }
            return result;
        }
    }
}
