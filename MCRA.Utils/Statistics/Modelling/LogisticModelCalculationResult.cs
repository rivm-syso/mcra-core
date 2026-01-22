namespace MCRA.Utils.Statistics.Modelling {

    public sealed class LogisticModelCalculationResult {
        public LogisticModelCalculationResult(
            List<double> estimates,
            double dispersion,
            double logLikelihood,
            double[] linearPredictor,
            double degreesOfFreedom,
            bool estimateDispersion = true
        ) {
            Estimates = estimates;
            Dispersion = dispersion;
            LogLikelihood = logLikelihood;
            LinearPredictor = linearPredictor;
            DegreesOfFreedom = degreesOfFreedom;
            EstimateDispersion = estimateDispersion;
        }

        /// <summary>
        /// Estimates of regression parameters.
        /// </summary>
        public List<double> Estimates { get; set; }

        /// <summary>
        /// Estimate of dispersion parameter.
        /// </summary>
        public double Dispersion { get; set; }

        /// <summary>
        /// Estimate dispersion parameter.
        /// </summary>
        public bool EstimateDispersion { get; set; }

        /// <summary>
        /// Log-likelihood.
        /// </summary>
        public double LogLikelihood { get; set; }

        /// <summary>
        /// Degrees of freedom.
        /// </summary>
        public double DegreesOfFreedom { get; set; }

        /// <summary>
        /// Linear predictor values.
        /// </summary>
        public double[] LinearPredictor { get; set; }

        /// <summary>
        /// Standard errors of regression parameters.
        /// </summary>
        public double[] StandardErrors { get; set; }

        /// <summary>
        /// Standard error of the dispersion.
        /// </summary>
        public double DispersionStandardError { get; set; } = double.NaN;
    }
}