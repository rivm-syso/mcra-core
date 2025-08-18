using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake air indoor fractions.
    /// </summary>
    public static class FakeAirIndoorFractionsGenerator {

        /// <summary>
        /// Generates fake air indoor fractions.
        /// </summary>
        public static List<AirIndoorFraction> Create(
            List<double?> ages,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var airIndoorFractions = new List<AirIndoorFraction>();
            var value = random.NextDouble(0.85, 0.99);
            var i = 0;
            foreach (var age in ages) {
                airIndoorFractions.Add(new AirIndoorFraction() {
                    idSubgroup = i.ToString(),
                    AgeLower = age,
                    Fraction = value
                });
                i++;
            }
            return airIndoorFractions;
        }
    }
}

