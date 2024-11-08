using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.UnitVariability;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock unit variability factors
    /// </summary>
    public static class FakeUnitVariabilityFactorsGenerator {
        /// <summary>
        /// Creates unit variability factors
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <param name="fractionMissing"></param>
        /// <returns></returns>
        public static Dictionary<Food, FoodUnitVariabilityInfo> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random,
            double fractionMissing = 0
        ) {
            var result = new Dictionary<Food, FoodUnitVariabilityInfo>();
            foreach (var food in foods) {
                if (random.NextDouble() <= fractionMissing) {
                    continue;
                }
                var unitVariabilityFactors = new List<UnitVariabilityFactor>();
                foreach (var substance in substances) {
                    var unitWeight = random.NextDouble() * 200;
                    unitVariabilityFactors.Add(new UnitVariabilityFactor(5, 7, unitWeight) {
                        Compound = substance,
                        Food = food,
                        UnitsInCompositeSample = random.NextDouble() * 100,
                        Factor = 5,
                        ProcessingType = new ProcessingType()
                    });
                }
                unitVariabilityFactors.Add(new UnitVariabilityFactor(5, 7, random.NextDouble() * 200) {
                    Food = food,
                    UnitsInCompositeSample = random.NextDouble() * 100,
                    Factor = 5,
                });
                result[food] = new FoodUnitVariabilityInfo(food, unitVariabilityFactors);
            }
            return result;
        }

        /// <summary>
        /// Creates unit variability factors
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Dictionary<Food, FoodUnitVariabilityInfo> Create(
                ICollection<Food> foods,
                IRandom random
            ) {
            var result = new Dictionary<Food, FoodUnitVariabilityInfo>();
            foreach (var food in foods) {
                var unitVariabilityFactors = new List<UnitVariabilityFactor>();
                var unitWeight = random.NextDouble() * 200;
                unitVariabilityFactors.Add(new UnitVariabilityFactor(5, 7, unitWeight) {
                    Compound = null,
                    Food = food,
                    UnitsInCompositeSample = random.NextDouble() * 100,
                    Factor = 5,
                    ProcessingType = new ProcessingType()
                });
                result[food] = new FoodUnitVariabilityInfo(food, unitVariabilityFactors);
            }
            return result;
        }

    }
}
