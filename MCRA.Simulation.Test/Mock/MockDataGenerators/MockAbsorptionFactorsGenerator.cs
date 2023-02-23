using System.Collections.Generic;
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
        public static IDictionary<(ExposureRouteType RouteType, Compound Substance), double> Create(ICollection<ExposureRouteType> routes, ICollection<Compound> substances) {
            var result = new Dictionary<(ExposureRouteType, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposureRouteType.Dietary) {
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
