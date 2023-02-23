namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ISubstanceTargetExposure : ISubstanceTargetExposureBase {

        /// <summary>
        /// The raw substance amount (without any RPF/membership correction).
        /// I.e., not a concentration but an absolute amount not divided by a 
        /// volume or unit.
        /// </summary>
        double SubstanceAmount { get; }

        /// <summary>
        /// The total (substance) substance amount corrected for RPF and 
        /// membership probability.
        /// </summary>
        double EquivalentSubstanceAmount(double rpf, double membershipProbability);
    }
}
