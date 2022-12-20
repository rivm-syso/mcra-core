using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock samples
    /// </summary>
    public static class MockModelledFoodsInfosGenerator {

        /// <summary>
        /// Generates mock substance sample statistics.
        /// </summary>
        /// <param name="modelledFoods"></param>
        /// <param name="substances"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), ModelledFoodInfo> Create(ICollection<Food> modelledFoods, ICollection<Compound> substances, int n = 10) {
            var samplesPerFoodCompound = new Dictionary<(Food, Compound), ModelledFoodInfo>();
            foreach (var fam in modelledFoods) {
                foreach (var substance in substances) {
                    samplesPerFoodCompound.Add((fam, substance), new ModelledFoodInfo() {
                        HasMeasurements = n > 0,
                        HasPositiveMeasurements = (n - (int)(n / 2D)) > 0
                    });
                }
            }
            return samplesPerFoodCompound;
        }
    }
}
