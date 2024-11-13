using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust availability fractions.
    /// </summary>
    public static class FakeDustAvailabilityFractionsGenerator {

        /// <summary>
        /// Generates fake dust availability fractions.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DustAvailabilityFraction> Create(
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustAvailabilityFraction = new List<DustAvailabilityFraction>();
            foreach (var substance in substances) {
                var value = random.NextDouble(0.1, 0.2);
                dustAvailabilityFraction.Add(new DustAvailabilityFraction() {
                    idSubgroup = substance.Code,
                    Substance = substance,
                    Sex = GenderType.Undefined,
                    Value = value
                });
            }
            return dustAvailabilityFraction;
        }
    }
}