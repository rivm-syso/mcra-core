using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake soil concentrations.
    /// </summary>
    public static class FakeSoilConcentrationsGenerator {

        /// <summary>
        /// Generates fake soil concentrations for the specified substances.
        /// </summary>
        public static List<SubstanceConcentration> Create(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var concentrations = new List<SubstanceConcentration>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                concentrations.Add(new SubstanceConcentration() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return concentrations;
        }
    }
}
