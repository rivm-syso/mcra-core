using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock ivive hazard characterisation models.
    /// </summary>
    public static class MockIviveHazardCharacterisationsGenerator {

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="exposureType"></param>
        /// <param name="safetyFactor"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IDictionary<Compound, IviveHazardCharacterisation> Create(
            ICollection<Compound> substances,
            ExposureType exposureType,
            double safetyFactor = 100,
            ExposureRouteType exposureRoute = ExposureRouteType.Dietary,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = substances.ToDictionary(s => s, s => {
                var dose = LogNormalDistribution.Draw(random, 2, 1);
                return CreateSingle(
                    s,
                    dose,
                    exposureType == ExposureType.Chronic
                        ? HazardCharacterisationType.Adi
                        : HazardCharacterisationType.Arfd,
                    exposureRoute,
                    safetyFactor
                );
            });
            return result;
        }

        /// <summary>
        /// Creates a random target hazard dose model for the specified substance.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="value"></param>
        /// <param name="hazardCharacterisationType"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="combinedAssessmentFactor"></param>
        /// <returns></returns>
        public static IviveHazardCharacterisation CreateSingle(
            Compound substance,
            double value,
            HazardCharacterisationType hazardCharacterisationType,
            ExposureRouteType exposureRoute = ExposureRouteType.Dietary,
            double combinedAssessmentFactor = 1
        ) {
            return new IviveHazardCharacterisation() {
                KineticConversionFactor =1,
                InternalHazardDose = value,
                Value = value,
                Substance = substance,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                MolBasedRpf = 222,
                ExposureRoute = exposureRoute,
                HazardCharacterisationType = hazardCharacterisationType


            };
        }
    }
}
