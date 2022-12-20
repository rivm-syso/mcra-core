namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {
    public sealed class DoseResponseRelation {

        /// <summary>
        /// The dose response model equation (if available).
        /// </summary>
        public string DoseResponseModelEquation { get; set; }

        /// <summary>
        /// The dose response model parameters (if available).
        /// </summary>
        public string DoseResponseModelParameterValues { get; set; }

        /// <summary>
        /// The critical effect size of the dose response model (if available).
        /// </summary>
        public double CriticalEffectSize { get; set; } = double.NaN;

    }
}
