namespace MCRA.Utils.Statistics.Modelling {

    /// <summary>
    /// Fits the logistic regression model to binomial data by means of Iterated
    /// Re-weighted Least Squares. The dispersion parameter can be set to a fixed
    /// value by means of DispersionFix.
    /// </summary>
    public class LogisticRegression {

        // ToDo: LogisticRegression What happens when there is aliasing? Things go wrong...

        /// <summary>
        /// Maximum number of cycles in IRLS algorithm (default 100).
        /// </summary>
        public int MaxCycle { get; set; }

        /// <summary>
        /// Convergence criterion in IRLS algorithm (default 1.0e-6).
        /// Operates on absolute difference in log-likelihood between cycles.
        /// </summary>
        public double ToleranceAbsolute { get; set; }

        /// <summary>
        /// Convergence criterion in IRLS algorithm (default 1.0e-6).
        /// Operates on relative difference in log-likelihood between cycles.
        /// </summary>
        public double ToleranceRelative { get; set; }

        /// <summary>
        /// FrequencyModelDispersion parameter (default 1); set to NaN to estimate the dispersion parameter.
        /// </summary>
        public double DispersionFix { get; set; }

        /// <summary>
        /// Estimates.
        /// </summary>
        public List<double> Estimates { get; private set; }

        /// <summary>
        /// Standard errors of estimates.
        /// </summary>
        public double[] Se { get; private set; }

        /// <summary>
        /// Variance-covariance matrix of estimates.
        /// </summary>
        public GeneralMatrix Vcovariance { get; private set; }

        /// <summary>
        /// Linear predictor (transformed scale).
        /// </summary>
        public double[] LinearPredictor { get; private set; }

        /// <summary>
        /// Fitted values (response scale).
        /// </summary>
        public double[] FittedValues { get; private set; }

        public double LogLik { get; set; } = double.NaN;

        public double Deviance { get; set; } = double.NaN;

        public double MeanDeviance { get; set; } = double.NaN;

        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Linear predictor for Predictions after a call to Predict.
        /// </summary>
        public double[] PredictLinearPredictor { get; private set; }

        /// <summary>
        /// Fitted values (on probability scale) for Predictions after a call to Predict.
        /// </summary>
        public double[] PredictFittedValues { get; private set; }

        /// <summary>
        /// Standard errors of Linear predictor for Predictions after a call to Predict.
        /// </summary>
        public double[] PredictLinearPredictorSe { get; private set; }

        /// <summary>
        /// Standard errors of Fitted values (on probability scale) for Predictions after a call to Predict.
        /// </summary>
        public double[] PredictFittedValuesSe { get; private set; }

        /// <summary>
        /// Fits a logistic regression model.
        /// </summary>
        /// <param name="response">Binomial response.</param>
        /// <param name="nbinomial">Binomial totals.</param>
        /// <param name="designMatrix">Designmatrix for regression (including the constant).</param>
        public void Fit(List<int> response, List<int> nbinomial, double[,] designMatrix) {
            try {
                var weights = Enumerable.Repeat(1D, response.Count).ToList();
                localFit(response, nbinomial, designMatrix, weights);
            }
            catch (Exception ee) {
                var msg = "Logistic Regression aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Fits a logistic regression model.
        /// </summary>
        /// <param name="response">Binomial response.</param>
        /// <param name="nbinomial">Binomial totals.</param>
        /// <param name="designMatrix">Designmatrix for regression (including the constant).</param>
        /// <param name="weights">Statistical Weights.</param>
        public void Fit(List<int> response, List<int> nbinomial, double[,] designMatrix, List<double> weights) {
            try {
                localFit(response, nbinomial, designMatrix, weights);
            }
            catch (Exception ee) {
                var msg = "Logistic Regression aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Forms predictions.
        /// </summary>
        /// <param name="predictionMatrix">Prediction matrix for regression (including the constant).</param>
        public void Predict(double[,] predictionMatrix) {
            try {
                if (predictionMatrix == null) {
                    var msg = "PredictionMatrix must be set in LogisticRegression.";
                    throw new Exception(msg);
                }
                if (Estimates == null) {
                    var msg = "No logistic regression model has been fitted. Prediction can not be formed.";
                    throw new Exception(msg);
                }
                var ncol = predictionMatrix.GetLength(1);
                if (ncol != Estimates.Count) {
                    var msg = "The number of columns of PredictionMatrix must equal the number of estimates.";
                    throw new Exception(msg);
                }
                var npred = predictionMatrix.GetLength(0);
                PredictLinearPredictor = new double[npred];
                PredictFittedValues = new double[npred];
                PredictLinearPredictorSe = new double[npred];
                PredictFittedValuesSe = new double[npred];
                for (int i = 0; i < npred; i++) {
                    var lp = 0.0;
                    var var = 0.0;
                    for (int j = 0; j < ncol; j++) {
                        lp += Estimates[j] * predictionMatrix[i, j];
                        for (int k = 0; k < ncol; k++) {
                            var += Vcovariance.GetElement(j, k) * predictionMatrix[i, j] * predictionMatrix[i, k];
                        }
                    }
                    PredictLinearPredictor[i] = lp;
                    var = Math.Sqrt(var);
                    PredictLinearPredictorSe[i] = var;
                    lp = UtilityFunctions.ILogit(lp);
                    PredictFittedValues[i] = lp;
                    PredictFittedValuesSe[i] = lp * (1.0 - lp) * var;
                }
            } catch (Exception ee) {
                var msg = "Predictions for Logistic Regression aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        private void localFit(List<int> response, List<int> nbinomial, double[,] designMatrix, List<double> weights) {
            // Do some basic checking
            if ((response == null) || (nbinomial == null) || (designMatrix == null) || (weights == null)) {
                var msg = "Response, Nbinomial, DesignMatrix and Weights must be set in LogisticRegression.";
                throw new Exception(msg);
            }
            var nresponse = response.Count;
            if ((nresponse != nbinomial.Count) || (nresponse != designMatrix.GetLength(0)) || (nresponse != weights.Count)) {
                var msg = "Response, Nbinomial DesignMatrix and Weights must have the same dimension in LogisticRegression.";
                throw new Exception(msg);
            }

            // Declaration of output structures
            var npredictor = designMatrix.GetLength(1);
            var estimates = new List<double>();            // these are local since it is used as 'out' below
            var se = new List<double>(); ;                   // these are local since it is used as 'out' below
            var covar = new double[npredictor, npredictor];   // these are local since it is used as 'out' below
            FittedValues = new double[nresponse];
            LinearPredictor = new double[nresponse];

            //Initialize local structures
            var py = new double[nresponse];
            var nbweight = new double[nresponse];
            var irlsZ = new double[nresponse];
            var irlsW = new double[nresponse];
            var logLikPrevious = double.MaxValue;
            var meanDev = 0.0;

            // Initial FittedValues and LinearPredictor defined by Data
            var logLik0 = 0.0;
            var dfTot = 0;
            for (int i = 0; i < nresponse; i++) {
                py[i] = Convert.ToDouble(response[i]) / Convert.ToDouble(nbinomial[i]);
                FittedValues[i] = (0.1 + 0.8 * py[i]);
                LinearPredictor[i] = UtilityFunctions.Logit(FittedValues[i]);
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
            MultipleLinearRegressionResult mlrResults = null;
            for (cycle = 0; cycle < MaxCycle; cycle++) {
                for (int i = 0; i < nresponse; i++) {
                    var tmp = FittedValues[i] * (1.0 - FittedValues[i]);
                    irlsZ[i] = LinearPredictor[i] + (py[i] - FittedValues[i]) / tmp;
                    irlsW[i] = nbweight[i] * tmp;
                }
                mlrResults = MultipleLinearRegressionCalculator.Compute(designMatrix, irlsZ.ToList(), irlsW.ToList());
                LinearPredictor = mlrResults.FittedValues.ToArray();
                meanDev = mlrResults.MeanDeviance;
                estimates = mlrResults.RegressionCoefficients;
                se = mlrResults.StandardErrors;

                LogLik = 0D;
                for (int i = 0; i < nresponse; i++) {
                    FittedValues[i] = UtilityFunctions.ILogit(LinearPredictor[i]);
                    if (weights[i] != 0.0) {
                        LogLik += nbweight[i] * (py[i] * Math.Log(FittedValues[i]) + (1.0 - py[i]) * Math.Log(1.0 - FittedValues[i]));
                    }
                }
                var LogLikDiff = Math.Abs(LogLik - logLikPrevious);
                if ((LogLikDiff < ToleranceAbsolute) || (-LogLikDiff / LogLik < ToleranceRelative)) {
                    break;
                } else {
                    logLikPrevious = LogLik;
                }
            }

            // End of IRLS algorithm; Finalize
            if (cycle == MaxCycle) {
                Error = "No convergence within the maximum number of cycles (" + MaxCycle.ToString() + ").";
            }
            for (int i = 0; i < nresponse; i++) {
                FittedValues[i] *= nbinomial[i];
            }

            // Copying of estimates and se is necessary because in the call to Rlib they are passed as out
            Deviance = 2 * (logLik0 - LogLik);
            MeanDeviance = Deviance / (dfTot - npredictor);
            Estimates = estimates;

            var factor = double.IsNaN(DispersionFix)
                ? MeanDeviance / meanDev
                : DispersionFix / meanDev;

            Se = new double[npredictor];
            for (int i = 0; i < npredictor; i++) {
                Se[i] = se[i] * Math.Sqrt(factor);
                for (int j = 0; j < npredictor; j++) {
                    covar[i, j] *= factor;
                }
            }
            Vcovariance = new GeneralMatrix(covar);
        }
    }
}
