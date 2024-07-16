using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock absorption factors
    /// </summary>
    public static class MockAbsorptionFactorsGenerator {
        /// <summary>
        /// Creates default absorption factors for routes and substances
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static List<KineticAbsorptionFactor> Create(ICollection<ExposurePathType> routes, ICollection<Compound> substances) {
            var result = new List<KineticAbsorptionFactor>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposurePathType.Oral) {
                        result.Add(new KineticAbsorptionFactor() { Substance = substance, ExposureRoute = route, AbsorptionFactor = 1});
                    } else {
                        result.Add(new KineticAbsorptionFactor() { Substance = substance, ExposureRoute = route, AbsorptionFactor = 0.1 });
                    }
                }
            }
            return result;
        }
    }
}
