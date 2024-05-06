using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {
    public sealed class KineticConversionFactorCalculator : IKineticConversionFactorCalculator {

        private readonly TargetLevelType _targetLevel;
        private readonly KineticModelCalculatorFactory _kineticModelCalculatorFactory;
        private readonly double _nominalBodyWeight;

        public KineticConversionFactorCalculator(
            TargetLevelType targetLevel,
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            double nominalBodyWeight
        ) {
            _targetLevel = targetLevel;
            _kineticModelCalculatorFactory = kineticModelCalculatorFactory;
            _nominalBodyWeight = nominalBodyWeight;
        }

        /// <summary>
        /// The hazard characterisation level.
        /// </summary>
        public TargetLevelType TargetDoseLevel {
            get {
                return _targetLevel;
            }
        }

        /// <summary>
        /// TargetDoseLevel: external (from internal to external), reverse dosimetry (backwards) kinetic model
        ///  - Dietary, kinetic model
        ///  - Oral, kinetic model
        ///  - Dermal, kinetic model
        ///  - Inhalation, kinetic model
        ///  - AtTarget ?????????????????
        /// TargetDoseLevel: internal (from external to internal), forward dosimetry kinetic model, 
        ///  - Dietary, kinetic model
        ///  - Oral, kinetic model
        ///  - Dermal, kinetic model
        ///  - Inhalation, kinetic model
        ///  - AtTarget ?????????????????
        /// </summary>
        /// <param name="testSystemHazardDose"></param>
        /// <param name="targetUnit"></param>
        /// <param name="substance"></param>
        /// <param name="testSystemSpecies"></param>
        /// <param name="testSystemOrgan"></param>
        /// <param name="testSystemExposureRoute"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double ComputeKineticConversionFactor(
            double testSystemHazardDose,
            TargetUnit targetUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRoute testSystemExposureRoute,
            ExposureType exposureType,
            IRandom generator
        ) {
            return ComputeKineticConversionFactor(
                testSystemHazardDose,
                targetUnit,
                substance,
                testSystemSpecies,
                testSystemOrgan,
                testSystemExposureRoute,
                exposureType,
                _targetLevel,
                generator
            );
        }

        /// <summary>
        /// TargetDoseLevel: external (from internal to external), reverse dosimetry (backwards) kinetic model
        ///  - Dietary, kinetic model
        ///  - Oral, kinetic model
        ///  - Dermal, kinetic model
        ///  - Inhalation, kinetic model
        ///  - AtTarget ?????????????????
        /// TargetDoseLevel: internal (from external to internal), forward dosimetry kinetic model, 
        ///  - Dietary, kinetic model
        ///  - Oral, kinetic model
        ///  - Dermal, kinetic model
        ///  - Inhalation, kinetic model
        ///  - AtTarget ?????????????????
        /// </summary>
        /// <param name="internalHazardDose"></param>
        /// <param name="targetUnit"></param>
        /// <param name="substance"></param>
        /// <param name="testSystemSpecies"></param>
        /// <param name="testSystemOrgan"></param>
        /// <param name="testSystemExposureRoute"></param>
        /// <param name="targetDoseLevelType"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double ComputeKineticConversionFactor(
            double internalHazardDose,
            TargetUnit targetUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRoute testSystemExposureRoute,
            ExposureType exposureType,
            TargetLevelType targetDoseLevelType,
            IRandom generator
        ) {
            //External, reverse dosimetry, backwards
            if (targetDoseLevelType == TargetLevelType.External) {
                if (testSystemExposureRoute == ExposureRoute.Undefined) {
                    // Test system target level is internal
                    var kineticModelCalculator = _kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(substance);
                    var externalDose = kineticModelCalculator
                        .Reverse(
                            internalHazardDose,
                            substance,
                            ExposurePathType.Dietary,
                            exposureType,
                            targetUnit.ExposureUnit,
                            _nominalBodyWeight,
                            generator
                        );
                    return externalDose / internalHazardDose;
                } else {
                    // Test system is on the same level as the target system (i.e., external)
                    // Dietary, Oral, Dermal, Inhalation
                    return 1;
                }
            } else {
                // Internal, forward dosimetry
                if (testSystemExposureRoute != ExposureRoute.Undefined) {
                    // Test system target level is external
                    var kineticModelCalculator = _kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(substance);
                    var doseAtTarget = kineticModelCalculator
                        .CalculateTargetDose(
                            internalHazardDose,
                            substance,
                            testSystemExposureRoute.GetExposurePath(),
                            exposureType,
                            targetUnit.ExposureUnit,
                            _nominalBodyWeight,
                            generator
                        );
                    return doseAtTarget / internalHazardDose;
                } else if (testSystemExposureRoute == ExposureRoute.Undefined) {
                    // Test system target level is at the same level as the target (i.e., internal)
                    return 1;
                } else {
                    // Undefined
                    return double.NaN;
                }
            }
        }
    }
}
