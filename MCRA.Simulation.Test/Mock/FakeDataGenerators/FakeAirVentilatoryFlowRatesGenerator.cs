using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake air ventilatory flow rates.
    /// </summary>
    public static class FakeAirVentilatoryFlowRatesGenerator {

        /// <summary>
        /// Generates fake air ventilatory flow rates.
        /// </summary>
        public static List<AirVentilatoryFlowRate> Create(
            List<GenderType> sexes,
            List<double?> ages,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var airVentilatoryFlowRates = new List<AirVentilatoryFlowRate>();
            var value = random.NextDouble(0.85, 0.99);
            var i = 0;
            foreach (var sex in sexes) {
                foreach (var age in ages) {
                    airVentilatoryFlowRates.Add(new AirVentilatoryFlowRate() {
                        idSubgroup = i.ToString(),
                        AgeLower = age,
                        Sex = sex,
                        Value = value
                    });
                    i++;
                }
            }
            return airVentilatoryFlowRates;
        }
    }
}