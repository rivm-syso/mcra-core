using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    public static class FakeBackgroundFoodConcentrationsGenerator {

        public static List<BackgroundFoodConcentration> Create(
            IEnumerable<Food> foods,
            IEnumerable<Compound> substances,
            double mean = 1,
            double upper = 2,
            double fractionMissing = 0.2,
            int seed = 1
        ) {
            var result = new List<BackgroundFoodConcentration>();
            var random = new McraRandomGenerator(seed);
            var distribution = LogNormalDistribution.FromMeanAndUpper(mean, upper);
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    if (random.NextDouble() > fractionMissing) {
                        var numSamples = random.Next(1, 20);
                        var record = new BackgroundFoodConcentration() {
                            Food = food,
                            Substance = substance,
                            Value = distribution.Draw(random),
                            ConcentrationUnit = ConcentrationUnit.mgPerKg
                        };
                        result.Add(record);
                    }
                }
            }
            return result;
        }
    }
}
