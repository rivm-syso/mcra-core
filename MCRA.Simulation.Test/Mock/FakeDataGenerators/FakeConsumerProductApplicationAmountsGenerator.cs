using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake consumer product application amounts.
    /// </summary>
    public static class FakeConsumerProductApplicationAmountsGenerator {

        /// <summary>
        /// Generates fake consumer product application amounts.
        /// </summary>
        public static List<ConsumerProductApplicationAmount> Create(
            List<ConsumerProduct> products,
            List<GenderType> sexes,
            List<double?> ages,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var cpApplicationAmounts = new List<ConsumerProductApplicationAmount>();
            foreach (var product in products) {
                foreach (var sex in sexes) {
                    foreach (var age in ages) {
                        var value = random.NextDouble(0.4, 0.99) * 2;
                        cpApplicationAmounts.Add(new ConsumerProductApplicationAmount() {
                            Product = product,
                            Sex = sex,
                            AgeLower = age,
                            Amount = value
                        });
                    }
                }
            }
            return cpApplicationAmounts;
        }
    }
}