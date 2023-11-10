using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {

    /// <summary>
    /// Interface for calculator classes for computing biological matrix 
    /// concentrations for a target matrix based on a concentration value 
    /// of a sampling method of another biological matrix using a simple 
    /// conversion factor that is the same for all biological matrices 
    /// other than the target biological matrix.
    /// </summary>
    public interface ITargetMatrixConversionCalculator {

        public ICollection<HbmSubstanceTargetExposure> GetTargetSubstanceExposure(
            HbmSubstanceTargetExposure sourceExposure,
            TargetUnit sourceExposureUnit
        );
    }
}
