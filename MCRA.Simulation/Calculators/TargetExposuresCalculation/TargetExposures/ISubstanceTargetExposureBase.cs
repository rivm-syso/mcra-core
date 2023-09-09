using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ISubstanceTargetExposureBase {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Substance { get; }

    }
}
