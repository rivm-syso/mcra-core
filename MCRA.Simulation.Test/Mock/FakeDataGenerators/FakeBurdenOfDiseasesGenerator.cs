using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake burden of diseases.
    /// </summary>
    public static class FakeBurdenOfDiseasesGenerator {

        public static BurdenOfDisease Create(
            Effect effect,
            Population population = null
        ) {
            var record = new BurdenOfDisease() {
                Effect = effect,
                Population = population,
                BodIndicator = BodIndicator.DALY,
                Value = 3
            };
            return record;
        }
    }
}
