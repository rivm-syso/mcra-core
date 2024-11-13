using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust concentration distributions.
    /// </summary>
    public static class FakeDustConcentrationDistributionsGenerator {

        /// <summary>
        /// Generates fake dust concentration distributions for the specified substances.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DustConcentrationDistribution> Create(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustConcentrationDistributions = new List<DustConcentrationDistribution>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                dustConcentrationDistributions.Add(new DustConcentrationDistribution() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return dustConcentrationDistributions;
        }
    }
}
