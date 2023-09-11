using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {
    public sealed class KineticConversionFactorCalculator : IKineticConversionFactorCalculator {

        private readonly TargetLevelType _targetDoseLevelType;
        private readonly KineticModelCalculatorFactory _kineticModelCalculatorFactory;
        private readonly double _nominalBodyWeight;

        public KineticConversionFactorCalculator(
            TargetLevelType targetDoseLevelType,
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            double nominalBodyWeight
        ) {
            _targetDoseLevelType = targetDoseLevelType;
            _kineticModelCalculatorFactory = kineticModelCalculatorFactory;
            _nominalBodyWeight = nominalBodyWeight;
        }

        /// <summary>
        /// The hazard characterisation level.
        /// </summary>
        public TargetLevelType TargetDoseLevel {
            get {
                return _targetDoseLevelType;
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
        /// <param name="intakeUnit"></param>
        /// <param name="substance"></param>
        /// <param name="testSystemSpecies"></param>
        /// <param name="testSystemOrgan"></param>
        /// <param name="testSystemExposureRoute"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double ComputeKineticConversionFactor(
            double testSystemHazardDose,
            TargetUnit intakeUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRouteType testSystemExposureRoute,
            ExposureType exposureType,
            IRandom generator
        ) {
            return ComputeKineticConversionFactor(
                testSystemHazardDose,
                intakeUnit,
                substance,
                testSystemSpecies,
                testSystemOrgan,
                testSystemExposureRoute,
                exposureType,
                _targetDoseLevelType,
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
        /// <param name="intakeUnit"></param>
        /// <param name="substance"></param>
        /// <param name="testSystemSpecies"></param>
        /// <param name="testSystemOrgan"></param>
        /// <param name="testSystemExposureRoute"></param>
        /// <param name="targetDoseLevelType"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double ComputeKineticConversionFactor(
            double internalHazardDose,
            TargetUnit intakeUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRouteType testSystemExposureRoute,
            ExposureType exposureType,
            TargetLevelType targetDoseLevelType,
            IRandom generator
        ) {
            //External, reverse dosimetry, backwards
            if (targetDoseLevelType == TargetLevelType.External) {
                if (testSystemExposureRoute == ExposureRouteType.AtTarget) {
                    var kineticModelCalculator = _kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(substance);
                    var relativeCompartmentWeight = kineticModelCalculator.GetNominalRelativeCompartmentWeight();
                    var externalDose = kineticModelCalculator
                        .Reverse(
                            internalHazardDose,
                            substance,
                            ExposureRouteType.Dietary,
                            exposureType,
                            intakeUnit.ExposureUnit,
                            _nominalBodyWeight,
                            relativeCompartmentWeight,
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
                if (testSystemExposureRoute != ExposureRouteType.AtTarget && testSystemExposureRoute != ExposureRouteType.Undefined) {
                    var kineticModelCalculator = _kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(substance);
                    var relativeCompartmentWeight = kineticModelCalculator.GetNominalRelativeCompartmentWeight();
                    var doseAtTarget = kineticModelCalculator
                        .CalculateTargetDose(
                            internalHazardDose,
                            substance,
                            testSystemExposureRoute,
                            exposureType,
                            intakeUnit.ExposureUnit,
                            _nominalBodyWeight,
                            relativeCompartmentWeight,
                            generator
                        );
                    return doseAtTarget / internalHazardDose;
                } else if (testSystemExposureRoute == ExposureRouteType.AtTarget) {
                    return 1;
                } else {
                    // Undefined
                    return double.NaN;
                }
            }
        }
    }
}
