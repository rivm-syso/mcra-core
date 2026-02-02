using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust concentrations.
    /// </summary>
    public static class FakeDustConcentrationsGenerator {

        /// <summary>
        /// Generates fake dust concentrations for the specified substances.
        /// </summary>
        public static List<SubstanceConcentration> Create(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustConcentrations = new List<SubstanceConcentration>();
            foreach (var substance in substances) {
                var conc = random.NextDouble();
                dustConcentrations.Add(new SubstanceConcentration() {
                    Substance = substance,
                    Concentration = conc
                });
            }
            return dustConcentrations;
        }
    }
}
