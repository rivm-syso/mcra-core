using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ISubstanceTargetExposureBase {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Substance { get; }

        /// <summary>
        /// The unit of the exposure value. 
        /// </summary>
        TargetUnit Unit { get; }
    }
}
