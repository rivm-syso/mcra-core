using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake air indoor and outdoor concentration distributions.
    /// </summary>
    public static class FakeAirConcentrationDistributionsGenerator {

        /// <summary>
        /// Generates fake air concentration distributions for the specified substances.
        /// </summary>
        public static List<AirConcentration> CreateIndoor(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var airConcentrations = new List<AirConcentration>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                airConcentrations.Add(new AirConcentration() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return airConcentrations;
        }

        // <summary>
        /// Generates fake air concentration distributions for the specified substances.
        /// </summary>
        public static List<AirConcentration> CreateOutdoor(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var airConcentrations = new List<AirConcentration>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                airConcentrations.Add(new AirConcentration() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return airConcentrations;
        }
    }
}
