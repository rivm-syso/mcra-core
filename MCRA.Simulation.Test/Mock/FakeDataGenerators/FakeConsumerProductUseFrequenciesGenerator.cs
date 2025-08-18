using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake consumer product exposure use frequencies.
    /// </summary>
    public static class FakeConsumerProductUseFrequenciesGenerator {

        /// <summary>
        /// Generates fake consumer product use frequencies.
        /// </summary>
        public static List<IndividualConsumerProductUseFrequency> Create(
            List<Individual> individuals,
            List<ConsumerProduct> products,
            int number = 5,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var cpUseFrequencies = new List<IndividualConsumerProductUseFrequency>();
            foreach (var product in products) {
                foreach (var individual in individuals) {
                    for (int i = 0; i < number; i++) {
                        var value = random.NextDouble(0.1, 0.99) * 2;
                        cpUseFrequencies.Add(new IndividualConsumerProductUseFrequency() {
                            Product = product,
                            Individual = individual,
                            Frequency = value
                        });
                    }
                }
            }
            return cpUseFrequencies;
        }
    }
}