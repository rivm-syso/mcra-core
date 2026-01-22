namespace MCRA.Utils.Statistics.Modelling {

    /// <summary>
    /// Fits the logistic regression model to binomial data by means of Iterated
    /// Re-weighted Least Squares. The dispersion parameter can be set to a fixed
    /// value by means of DispersionFix.
    /// </summary>
    public class LogisticRegressionCalculator {

        // ToDo: LogisticRegression What happens when there is aliasing? Things go wrong...

        /// <summary>
        /// Maximum number of cycles in IRLS algorithm (default 100).
        /// </summary>
        private const int MaxCycle = 100;

        /// <summary>
        /// Convergence criterion in IRLS algorithm (default 1.0e-6).
        /// Operates on absolute difference in log-likelihood between cycles.
        /// </summary>
        private const double ToleranceAbsolute = 1e-6;

        /// <summary>
        /// Convergence criterion in IRLS algorithm (default 1.0e-6).
        /// Operates on relative difference in log-likelihood between cycles.
        /// </summary>
        private const double ToleranceRelative = 1e-6;

        /// <summary>
        /// Fits a logistic regression model.
        /// </summary>
        /// <param name="response">Binomial response.</param>
        /// <param name="nbinomial">Binomial totals.</param>
        /// <param name="designMatrix">Designmatrix for regression (including the constant).</param>
        /// <param name="weights">Statistical Weights.</param>
        public static List<double> Compute(
            List<int> response,
            List<int> nbinomial,
            double[,] designMatrix,
            List<double> weights
        ) {
            // Do some basic checking
            if ((response == null) || (nbinomial == null) || (designMatrix == null) || (weights == null)) {
                throw new Exception("Response, Nbinomial, DesignMatrix and Weights must be set in LogisticRegression.");
            }
            var nresponse = response.Count;
            if ((nresponse != nbinomial.Count) || (nresponse != designMatrix.GetLength(0)) || (nresponse != weights.Count)) {
                throw new Exception("Response, Nbinomial DesignMatrix and Weights must have the same dimension in LogisticRegression.");
            }

            // Declaration of output structures
            var estimates = new List<double>();
            var fittedValues = new double[nresponse];
            var linearPredictor = new double[nresponse];

            //Initialize local structures
            var py = new double[nresponse];
            var nbweight = new double[nresponse];
            var irlsZ = new double[nresponse];
            var irlsW = new double[nresponse];
            var logLikPrevious = double.MaxValue;

            // Initial FittedValues and LinearPredictor defined by Data
            var logLik0 = 0.0;
            var dfTot = 0;
            for (int i = 0; i < nresponse; i++) {
                py[i] = Convert.ToDouble(response[i]) / Convert.ToDouble(nbinomial[i]);
                fittedValues[i] = (0.1 + 0.8 * py[i]);
                linearPredictor[i] = UtilityFunctions.Logit(fittedValues[i]);
                nbweight[i] = weights[i] * nbinomial[i];
                if (weights[i] != 0.0) {
                    dfTot += 1;
                    if ((py[i] != 0.0) && (py[i] != 1.0)) {
                        logLik0 += nbweight[i] * (py[i] * Math.Log(py[i]) + (1 - py[i]) * Math.Log(1 - py[i]));
                    }
                }
            }

            // Weighted IRLS algorithm
            int cycle;
            for (cycle = 0; cycle < MaxCycle; cycle++) {
                for (int i = 0; i < nresponse; i++) {
                    var tmp = fittedValues[i] * (1.0 - fittedValues[i]);
                    irlsZ[i] = linearPredictor[i] + (py[i] - fittedValues[i]) / tmp;
                    irlsW[i] = nbweight[i] * tmp;
                }
                var mlrResults = MultipleLinearRegressionCalculator.Compute(designMatrix, [.. irlsZ], [.. irlsW]);
                linearPredictor = [.. mlrResults.FittedValues];
                estimates = mlrResults.RegressionCoefficients;

                var logLik = 0D;
                for (int i = 0; i < nresponse; i++) {
                    fittedValues[i] = UtilityFunctions.ILogit(linearPredictor[i]);
                    if (weights[i] != 0.0) {
                        logLik += nbweight[i] * (py[i] * Math.Log(fittedValues[i]) + (1.0 - py[i]) * Math.Log(1.0 - fittedValues[i]));
                    }
                }
                var logLikDiff = Math.Abs(logLik - logLikPrevious);
                if ((logLikDiff < ToleranceAbsolute) || (-logLikDiff / logLik < ToleranceRelative)) {
                    break;
                } else {
                    logLikPrevious = logLik;
                }
            }
            return estimates;
        }
    }
}
