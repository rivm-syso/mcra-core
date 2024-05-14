using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ISubstanceTargetExposure {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Substance { get; }

        /// <summary>
        /// The substance exposure (without any RPF/membership correction).
        /// Depending on the target exposure unit this may be an absolute amount
        /// or a concentration.
        /// </summary>
        double Exposure { get; }

        /// <summary>
        /// The total (substance) substance amount corrected for RPF and 
        /// membership probability.
        /// </summary>
        double EquivalentSubstanceExposure(double rpf, double membershipProbability);

    }
}
