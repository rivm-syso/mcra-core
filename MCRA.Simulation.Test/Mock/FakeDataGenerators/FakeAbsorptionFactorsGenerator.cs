﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake absorption factors.
    /// </summary>
    public static class FakeAbsorptionFactorsGenerator {

        /// <summary>
        /// Creates default absorption factors for routes and substances.
        /// </summary>
        public static List<SimpleAbsorptionFactor> Create(
            ICollection<ExposurePathType> routes,
            ICollection<Compound> substances
        ) {
            var result = new List<SimpleAbsorptionFactor>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposurePathType.Oral) {
                        result.Add(new SimpleAbsorptionFactor() {
                            Substance = substance,
                            ExposureRoute = route,
                            AbsorptionFactor = 1
                        });
                    } else {
                        result.Add(new SimpleAbsorptionFactor() {
                            Substance = substance,
                            ExposureRoute = route,
                            AbsorptionFactor = 0.1
                        });
                    }
                }
            }
            return result;
        }
    }
}