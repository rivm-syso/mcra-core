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
        public static IDictionary<(ExposurePathType RouteType, Compound Substance), double> Create(ICollection<ExposurePathType> routes, ICollection<Compound> substances) {
            var result = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposurePathType.Dietary) {
                        result[(route, substance)] = 1;
                    } else {
                        result[(route, substance)] = 0.1;
                    }
                }
            }
            return result;
        }
    }
}
