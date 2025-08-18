using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake consumer product concentration.
    /// </summary>
    public static class FakeConsumerProductConcentrationsGenerator {

        /// <summary>
        /// Generates fake dust concentration for the specified substances.
        /// </summary>
        public static List<ConsumerProductConcentration> Create(
            List<Compound> substances,
            List<ConsumerProduct> products,
            int number = 10,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var cpConcentrations = new List<ConsumerProductConcentration>();
            foreach (var substance in substances) {
                foreach (var product in products) {
                    for (int i = 0; i < number; i++) {
                        var conc = random.NextDouble();
                        cpConcentrations.Add(new ConsumerProductConcentration() {
                            Substance = substance,
                            Product = product,
                            Concentration = conc
                        });
                    }
                }
            }
            return cpConcentrations;
        }
    }
}
