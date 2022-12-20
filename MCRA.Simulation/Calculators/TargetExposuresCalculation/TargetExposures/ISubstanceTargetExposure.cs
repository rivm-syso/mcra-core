using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ISubstanceTargetExposure {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Substance { get; }

        /// <summary>
        /// The raw substance concentration (without any RPF/membership correction)
        /// </summary>
        double SubstanceAmount { get; }

        /// <summary>
        /// The total (substance) substance amount corrected for RPF and membership probability.
        /// </summary>
        double EquivalentSubstanceAmount(double rpf, double membershipProbability);

    }
}
