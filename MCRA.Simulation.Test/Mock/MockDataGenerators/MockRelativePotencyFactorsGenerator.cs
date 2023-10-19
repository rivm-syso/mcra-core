using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock relative potency factors.
    /// </summary>
    public static class MockRelativePotencyFactorsGenerator {

        /// <summary>
        /// Creates a list of relative potency factors for the provided substances.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="reference"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<RelativePotencyFactor> Create(
            IEnumerable<Compound> substances,
            Compound reference,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = substances
                .Select(r => new RelativePotencyFactor() {
                    Compound = r,
                    RPF = r == reference
                        ? 1D
                        : 1D / LogNormalDistribution.Draw(random, 2, 1)
                })
                .ToList();
            return result;
        }

    }
}
