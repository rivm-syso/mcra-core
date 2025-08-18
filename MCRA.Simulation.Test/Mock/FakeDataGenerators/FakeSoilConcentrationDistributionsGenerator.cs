using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake soil concentration distributions.
    /// </summary>
    public static class FakeSoilConcentrationDistributionsGenerator {

        /// <summary>
        /// Generates fake soil concentration distributions for the specified substances.
        /// </summary>
        public static List<SoilConcentrationDistribution> Create(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var concentrationDistributions = new List<SoilConcentrationDistribution>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                concentrationDistributions.Add(new SoilConcentrationDistribution() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return concentrationDistributions;
        }
    }
}
