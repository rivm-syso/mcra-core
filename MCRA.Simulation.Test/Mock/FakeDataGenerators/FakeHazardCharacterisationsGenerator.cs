using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake hazard characterisations.
    /// </summary>
    public static class FakeHazardCharacterisationsGenerator {

        public static IDictionary<Compound, HazardCharacterisation> CreateExternal(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            ExposurePathType exposureRoute = ExposurePathType.Oral,
            DoseUnit doseUnit = DoseUnit.mgPerKgBWPerDay,
            bool isCriticalEffect = true,
            double safetyFactor = 100,
            int seed = 1
        ) {
            return Create(
                substances,
                effect,
                exposureType,
                TargetLevelType.External,
                exposureRoute,
                doseUnit: doseUnit,
                isCriticalEffect: isCriticalEffect,
                safetyFactor: safetyFactor,
                seed: seed
            );
        }

        public static IDictionary<Compound, HazardCharacterisation> CreateInternal(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType,
            DoseUnit doseUnit = DoseUnit.ugPerL,
            bool isCriticalEffect = true,
            double safetyFactor = 100,
            int seed = 1
        ) {
            return Create(
                substances,
                effect,
                exposureType,
                TargetLevelType.Internal,
                biologicalMatrix: biologicalMatrix,
                expressionType: expressionType,
                doseUnit: doseUnit,
                isCriticalEffect: isCriticalEffect,
                safetyFactor: safetyFactor,
                seed: seed
            );
        }

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        public static IDictionary<Compound, HazardCharacterisation> Create(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            TargetLevelType targetLevel = TargetLevelType.External,
            ExposurePathType exposureRoute = ExposurePathType.Oral,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Undefined,
            ExpressionType expressionType = ExpressionType.None,
            DoseUnit doseUnit = DoseUnit.mgPerKgBWPerDay,
            bool isCriticalEffect = true,
            double safetyFactor = 100,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = substances.ToDictionary(s => s, s => {
                var dose = LogNormalDistribution.Draw(random, 2, 1);
                return CreateSingle(
                    s,
                    effect,
                    dose,
                    doseUnit,
                    exposureType,
                    targetLevel,
                    exposureType == ExposureType.Chronic
                        ? HazardCharacterisationType.Adi
                        : HazardCharacterisationType.Arfd,
                    exposureRoute,
                    biologicalMatrix,
                    expressionType,
                    isCriticalEffect,
                    safetyFactor
                );
            });
            return result;
        }

        /// <summary>
        /// Creates a random target hazard dose model for the specified substance, for external hazard characterisations.
        /// </summary>
        public static HazardCharacterisation CreateSingle(
            Compound substance,
            Effect effect,
            double value,
            DoseUnit doseUnit,
            ExposureType exposureType,
            TargetLevelType targetLevel,
            HazardCharacterisationType hazardCharacterisationType,
            ExposurePathType exposureRoute,
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType,
            bool isCriticalEffect,
            double combinedAssessmentFactor
        ) {
            return new HazardCharacterisation() {
                Effect = effect,
                Value = value,
                IsCriticalEffect = isCriticalEffect,
                ExposureType = exposureType,
                DoseUnit = doseUnit,
                Substance = substance,
                ExposureRoute = exposureRoute.GetExposureRoute(),
                TargetLevel = targetLevel,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                HazardCharacterisationType = hazardCharacterisationType,
                BiologicalMatrix = biologicalMatrix,
                ExpressionType = expressionType,
            };
        }
    }
}
