using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust adherence amounts.
    /// </summary>
    public static class FakeDustAdherenceAmountsGenerator {

        /// <summary>
        /// Generates fake dust adherence amounts.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DustAdherenceAmount> Create(
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustAdherenceAmounts = new List<DustAdherenceAmount>();
            var value = random.NextDouble(0.85, 0.95);
            dustAdherenceAmounts.Add(new DustAdherenceAmount() {
                idSubgroup = "1",
                Sex = GenderType.Undefined,
                Value = value
            });
            return dustAdherenceAmounts;
        }
    }
}