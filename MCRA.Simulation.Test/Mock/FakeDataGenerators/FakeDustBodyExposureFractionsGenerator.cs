using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust body exposure fractions.
    /// </summary>
    public static class FakeDustBodyExposureFractionsGenerator {

        /// <summary>
        /// Generates fake dust body exposure fractions.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DustBodyExposureFraction> Create(
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustBodyExposureFractions = new List<DustBodyExposureFraction>();
            List<GenderType> sexes = [GenderType.Female, GenderType.Male];
            foreach (var sex in sexes) {
                var value = random.NextDouble(0.6, 0.7);
                dustBodyExposureFractions.Add(new DustBodyExposureFraction() {
                    idSubgroup = sex.GetTypeCode().ToString(),
                    Sex = sex,
                    Value = value
                });
            }
            return dustBodyExposureFractions;
        }
    }
}