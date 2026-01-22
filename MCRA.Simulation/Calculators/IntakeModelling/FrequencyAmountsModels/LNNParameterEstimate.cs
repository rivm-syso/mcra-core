namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Represents a set of statistical parameters used for modeling frequency and
    /// amount data, including transformation variance, and correlation values.
    /// This set of parameters is always present in LNN models
    /// </summary>
    public class LNNParameterEstimate {
        /// <summary>
        /// Power transformation parameter (0 is for the Log transform).
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// Between individuals variance for frequency model
        /// </summary>
        public double Dispersion { get; set; }

        /// <summary>
        /// Between individuals variance for amount model
        /// </summary>
        public double VarianceBetween { get; set; }

        /// <summary>
        /// Between days within individuals variance for amount model
        /// </summary>
        public double VarianceWithin { get; set; }

        /// <summary>
        /// Correlation between individual effects of frequency and amounts
        /// </summary>
        public double Correlation { get; set; }

    }
}
