using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating fake foods
    /// </summary>
    public static class MockFoodsGenerator {

        private static string[] _defaultFoods = {
            "Apples", "Bananas", "Cherries",
            "Dates", "Fig", "Grapefruit",
            "Hackberry", "Imbe", "Jambolan",
            "Kiwi", "Lime", "Mango",
            "Nectarine", "Oranges", "Peach",
            "Quince", "Raspberries", "Strawberries",
            "Tangerine", "Ugni", "Voavanga",
            "Watermelon", "Xigua", "Yangmei",
            "Zuchinni"
        };

        /// <summary>
        /// Creates a list of foods
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<Food> MockFoods(params string[] names) {
            var result = names
                .Select(r => new Food() {
                    Code = r,
                    Name = r,
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of foods
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<Food> Create(int n) {
            if (n <= _defaultFoods.Length) {
                var result = _defaultFoods
                    .Take(n)
                    .Select(r => new Food() {
                        Code = r,
                        Name = r,
                        Properties = new FoodProperty() { UnitWeight = 100 },
                    })
                    .ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultFoods.Length} mock foods using this method!");
        }

        /// <summary>
        /// Creates processed foods / processed derivatives from the
        /// provided (base) foods and specified processing types.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="processingTypes"></param>
        /// <returns></returns>
        public static List<Food> CreateProcessedFoods(
            ICollection<Food> foods,
            ICollection<ProcessingType> processingTypes
        ) {
            var result = foods
                .SelectMany(
                    f => processingTypes,
                    (f, p) => new Food() {
                        Code = $"{f.Code}-{p.Code}",
                        Name = $"{f.Name} - {p.Name}",
                        BaseFood = f,
                        ProcessingTypes = new List<ProcessingType>() { p }
                    }
                )
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates foods with unit weights.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="fractionMissing"></param>
        /// <param name="locations"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<Food> CreateFoodsWithUnitWeights(
            int number,
            IRandom random,
            double fractionMissing = .2,
            string[] locations = null
        ) {
            var foods = Create(number);
            var counter = 0;
            foreach (var food in foods) {
                var defaultUwRac = LogNormalDistribution.Draw(random, 4.2, .9);
                var defaultUwEp = (.9 + .1 * BetaDistribution.Draw(random, 0.5, 0.5)) * defaultUwRac;
                if (random.NextDouble() > fractionMissing) {
                    food.DefaultUnitWeightRac = new FoodUnitWeight() {
                        Food = food,
                        Qualifier = defaultUwRac < 25 ? ValueQualifier.LessThan : ValueQualifier.Equals,
                        Value = defaultUwRac < 25 ? 25 : defaultUwRac,
                        ValueType = UnitWeightValueType.UnitWeightRac
                    };
                }
                if (random.NextDouble() > fractionMissing) {
                    food.DefaultUnitWeightEp = new FoodUnitWeight() {
                        Food = food,
                        Qualifier = defaultUwEp < 25 ? ValueQualifier.LessThan : ValueQualifier.Equals,
                        Value = defaultUwEp < 25 ? 25 : defaultUwEp,
                        ValueType = UnitWeightValueType.UnitWeightEp
                    };
                }
                if (locations != null) {
                    foreach (var location in locations) {
                        var locationUwRac = defaultUwRac * (.9 + .2 * random.NextDouble());
                        var locationUwEp = (.9 + .1 * BetaDistribution.Draw(random, 0.5, 0.5)) * locationUwRac;
                        if (random.NextDouble() > fractionMissing) {
                            food.FoodUnitWeights.Add(new FoodUnitWeight() {
                                Food = food,
                                Location = location,
                                Qualifier = locationUwRac < 25 ? ValueQualifier.LessThan : ValueQualifier.Equals,
                                Value = locationUwRac < 25 ? 25 : locationUwRac,
                                ValueType = UnitWeightValueType.UnitWeightRac
                            });
                        }
                        if (random.NextDouble() > fractionMissing) {
                            food.FoodUnitWeights.Add(new FoodUnitWeight() {
                                Food = food,
                                Location = location,
                                Qualifier = locationUwEp < 25 ? ValueQualifier.LessThan : ValueQualifier.Equals,
                                Value = locationUwEp < 25 ? 25 : locationUwEp,
                                ValueType = UnitWeightValueType.UnitWeightEp
                            });
                        }
                    }
                }
                counter++;
            }
            return foods;
        }

        /// <summary>
        /// Creates reverse yield factors for the processed foods of the food collection.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IDictionary<(Food, string), double> CreateReverseYieldFactors(
            ICollection<Food> foods,
            IRandom random
        ) {
            IDictionary<(Food, string), double> processingProportions = new Dictionary<(Food, string), double>();
            var records = foods
                .Where(r => r.BaseFood != null && r.ProcessingFacetCode() != null)
                .Select(f => (f.BaseFood, f.ProcessingFacetCode(), random.NextDouble()))
                .ToList();
            records.ForEach(r => processingProportions[(r.BaseFood, r.Item2)] = r.Item3);
            return processingProportions;
        }
    }
}
