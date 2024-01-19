using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {
    public interface IKineticConversionFactorCalculator {

        TargetLevelType TargetDoseLevel { get; }

        double ComputeKineticConversionFactor(
            double testSystemHazardDose,
            TargetUnit targetUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRoute testSystemExposureRoute,
            ExposureType exposureType,
            IRandom generator
        );

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
        /// <param name="dose"></param>
        /// <param name="targetUnit"></param>
        /// <param name="substance"></param>
        /// <param name="testSystemSpecies"></param>
        /// <param name="testSystemOrgan"></param>
        /// <param name="testSystemExposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="targetDoseLevelType"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        double ComputeKineticConversionFactor(
            double dose,
            TargetUnit targetUnit,
            Compound substance,
            string testSystemSpecies,
            string testSystemOrgan,
            ExposureRoute testSystemExposureRoute,
            ExposureType exposureType,
            TargetLevelType targetDoseLevelType,
            IRandom generator
        );
    }
}
