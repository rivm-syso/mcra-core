using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock target hazard dose models
    /// </summary>
    public static class MockHazardCharacterisationsGenerator {

        public static IDictionary<Compound, HazardCharacterisation> Create(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            double safetyFactor = 100,
            ExposurePathType exposureRoute = ExposurePathType.Dietary,
            bool isCriticalEffect = true,
            int seed = 1
            ) {
                return Create(substances, effect, exposureType, safetyFactor, exposureRoute, TargetLevelType.External, DoseUnit.mgPerKgBWPerDay, BiologicalMatrix.WholeBody, ExpressionType.None, isCriticalEffect, seed);
        }

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        public static IDictionary<Compound, HazardCharacterisation> Create(
            ICollection<Compound> substances,
            Effect effect,
            ExposureType exposureType,
            double safetyFactor = 100,
            ExposurePathType exposureRoute = ExposurePathType.Dietary,
            TargetLevelType targetLevel = TargetLevelType.External,
            DoseUnit doseUnit = DoseUnit.mgPerKgBWPerDay,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.WholeBody,
            ExpressionType expressionType = ExpressionType.None,
            bool isCriticalEffect = true,
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
            ExposurePathType exposureRoute = ExposurePathType.Dietary,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.WholeBody,
            ExpressionType expressionType = ExpressionType.None,
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
                TargetLevel = targetLevel,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                HazardCharacterisationTypeString = hazardCharacterisationType.ToString(),
                BiologicalMatrix = biologicalMatrix,
                ExpressionType = expressionType,
            };
        }
    }
}
