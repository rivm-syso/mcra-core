﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock ivive hazard characterisation models.
    /// </summary>
    public static class FakeIviveHazardCharacterisationsGenerator {

        /// <summary>
        /// Creates a dictionary of target hazard dose model for each substance
        /// </summary>
        public static IDictionary<Compound, IviveHazardCharacterisation> Create(
            ICollection<Compound> substances,
            ExposureType exposureType,
            TargetUnit targetUnit,
            TargetUnit internalTargetUnit,
            double safetyFactor = 100,
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
                    targetUnit,
                    internalTargetUnit,
                    safetyFactor
                );
            });
            return result;
        }

        /// <summary>
        /// Creates a random target hazard dose model for the specified substance.
        /// </summary>
        public static IviveHazardCharacterisation CreateSingle(
            Compound substance,
            double value,
            HazardCharacterisationType hazardCharacterisationType,
            TargetUnit targetUnit,
            TargetUnit internalTargetUnit,
            double combinedAssessmentFactor = 1
        ) {
            return new IviveHazardCharacterisation() {
                KineticConversionFactor =1,
                InternalHazardDose = value,
                Value = value,
                Substance = substance,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                TargetUnit = targetUnit,
                InternalTargetUnit = internalTargetUnit,
                HazardCharacterisationType = hazardCharacterisationType
            };
        }
    }
}
