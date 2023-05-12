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
    public interface IBiologicalMatrixConcentrationConversionCalculator {

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        /// <param name="sourceSamplingMethod"></param>
        /// <param name="targetBiologicalMatrix"></param>
        /// <param name="concentration"></param>
        /// <returns></returns>
        double GetTargetConcentration(
            HumanMonitoringSamplingMethod sourceSamplingMethod,
            BiologicalMatrix targetBiologicalMatrix,
            double concentration
        );
    }
}
