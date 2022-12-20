using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;

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
        public static TwoKeyDictionary<ExposureRouteType, Compound, double> Create(ICollection<ExposureRouteType> routes, ICollection<Compound> substances) {
            var result = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposureRouteType.Dietary) {
                        result[route, substance] = 1;
                    } else {
                        result[route, substance] = 0.1;
                    }
                }
            }
            return result;
        }
    }
}
