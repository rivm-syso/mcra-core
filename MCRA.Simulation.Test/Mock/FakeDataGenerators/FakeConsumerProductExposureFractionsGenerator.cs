using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake consumer product exposure fractions.
    /// </summary>
    public static class FakeConsumerProductExposureFractionsGenerator {

        /// <summary>
        /// Generates fake consumer product exposure fractions.
        /// </summary>
        public static List<ConsumerProductExposureFraction> Create(
            List<Compound> substances,
            List<ConsumerProduct> products,
            List<ExposureRoute> routes,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var cpExposureFraction = new List<ConsumerProductExposureFraction>();
            foreach (var substance in substances) {
                foreach (var product in products) {
                    foreach (var route in routes) {
                        var value = random.NextDouble(0.1, 0.99);
                        cpExposureFraction.Add(new ConsumerProductExposureFraction() {
                            Product = product,
                            Substance = substance,
                            Route = route,
                            ExposureFraction = value
                        });
                    }
                }
            }
            return cpExposureFraction;
        }
    }
}