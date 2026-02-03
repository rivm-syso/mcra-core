using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake absorption factors.
    /// </summary>
    public static class FakeAbsorptionFactorsGenerator {

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static IDictionary<(ExposureRoute, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            double value
        ) {
            var kineticConversionFactors = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var substance in substances) {
                kineticConversionFactors[(ExposureRoute.Dermal, substance)] = value;
                kineticConversionFactors[(ExposureRoute.Oral, substance)] = value;
                kineticConversionFactors[(ExposureRoute.Inhalation, substance)] = value;
            }
            return kineticConversionFactors;
        }

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static IDictionary<(ExposureRoute, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double value
        ) {
            var kineticConversionFactors = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    kineticConversionFactors[(route, substance)] = value;
                }
            }
            return kineticConversionFactors;
        }

        /// <summary>
        /// Creates default absorption factors for routes and substances.
        /// </summary>
        public static List<SimpleAbsorptionFactor> Create(
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances
        ) {
            var result = new List<SimpleAbsorptionFactor>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    if (route == ExposureRoute.Oral) {
                        result.Add(new SimpleAbsorptionFactor() {
                            Substance = substance,
                            ExposurePathType = route.GetExposurePath(),
                            AbsorptionFactor = 1
                        });
                    } else {
                        result.Add(new SimpleAbsorptionFactor() {
                            Substance = substance,
                            ExposurePathType = route.GetExposurePath(),
                            AbsorptionFactor = 0.1
                        });
                    }
                }
            }
            return result;
        }
    }
}
