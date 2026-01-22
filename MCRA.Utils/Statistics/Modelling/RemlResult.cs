namespace MCRA.Utils.Statistics.Modelling {

    /// <summary>
    /// Summarizes results of lme4 (R).
    /// </summary>
    public sealed class RemlResult {

        public double VarianceBetween { get; set; }

        public double VarianceWithin { get; set; }

        /// <summary>
        /// This component is only useful for acute with covariate modelling
        /// and fit is fixed model + blup (fit = mu + u).
        /// </summary>
        public double VarianceDistribution { get; set; } = double.NaN;

        public List<double> Estimates { get; set; }

        /// <summary>
        /// Standard errors
        /// </summary>
        public List<double> Se { get; set; }

        /// <summary>
        /// Degrees of freedom.
        /// </summary>
        public int Df { get; set; }

        /// <summary>
        /// -2 * log-likelihood
        /// </summary>
        public double _2LogLikelihood { get; set; }

        /// <summary>
        /// Random effects for individuals.
        /// </summary>
        public List<double> Blups { get; set; }

        /// <summary>
        /// Random effects for individuals and days, e[i] + e[ij].
        /// </summary>
        public List<double> Residuals { get; set; }

        /// <summary>
        /// Fitted contains fixed effects and random (blup) effects.
        /// </summary>
        public List<double> FittedValues { get; set; }
    }
}
