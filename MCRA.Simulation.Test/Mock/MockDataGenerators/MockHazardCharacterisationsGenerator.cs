using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock target hazard dose models
    /// </summary>
    public static class MockHazardCharacterisationsGenerator {

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="exposureType"></param>
        /// <param name="safetyFactor"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="isCriticalEffect"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IDictionary<Compound, HazardCharacterisation> Create(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            double safetyFactor = 100,
            ExposureRouteType exposureRoute = ExposureRouteType.Dietary,
            bool isCriticalEffect = true,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var doseUnit = DoseUnit.mgPerKgBWPerDay;
            var result = substances.ToDictionary(s => s, s => {
                var dose = LogNormalDistribution.Draw(random, 2, 1);
                return CreateSingle(
                    s,
                    effect,
                    dose,
                    doseUnit,
                    exposureType,
                    exposureType == ExposureType.Chronic
                        ? HazardCharacterisationType.Adi
                        : HazardCharacterisationType.Arfd,
                    exposureRoute,
                    isCriticalEffect,
                    safetyFactor
                );
            });
            return result;
        }

        /// <summary>
        /// Creates a random target hazard dose model for the specified substance.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="effect"></param>
        /// <param name="value"></param>
        /// <param name="doseUnit"></param>
        /// <param name="exposureType"></param>
        /// <param name="hazardCharacterisationType"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="isCriticalEffect"></param>
        /// <param name="combinedAssessmentFactor"></param>
        /// <returns></returns>
        public static HazardCharacterisation CreateSingle(
            Compound substance,
            Effect effect,
            double value,
            DoseUnit doseUnit,
            ExposureType exposureType,
            HazardCharacterisationType hazardCharacterisationType,
            ExposureRouteType exposureRoute = ExposureRouteType.Dietary,
            bool isCriticalEffect = true,
            double combinedAssessmentFactor = 1
        ) {
            return new HazardCharacterisation() {
                Effect = effect,
                Value = value,
                IsCriticalEffect = isCriticalEffect,
                ExposureTypeString = exposureType.ToString(),
                DoseUnitString = doseUnit.ToString(),
                Substance = substance,
                ExposureRoute = exposureRoute,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                HazardCharacterisationTypeString = hazardCharacterisationType.ToString()
            };
        }
    }
}
