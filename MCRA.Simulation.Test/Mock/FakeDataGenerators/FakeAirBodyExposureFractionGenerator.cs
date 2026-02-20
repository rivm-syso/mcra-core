using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake air body exposure fractions.
    /// </summary>
    public static class FakeAirBodyExposureFractionGenerator {

        /// <summary>
        /// Generates fake air body exposure fractions.
        /// </summary>
        public static List<AirBodyExposureFraction> Create() {
            return [
                new() {
                    Value = .3,
                    Sex = GenderType.Undefined
                }
            ];
        }
    }
}