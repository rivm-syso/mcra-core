using MCRA.Utils.R.REngines;

namespace MCRA.Utils.Statistics.Modelling {

    public sealed class MultipleLinearRegressionCalculator {

        /// <summary>
        /// Performs a general multiple linear regression when the independent variables may be linearly dependent. 
        /// Parameter estimates, standard errors, residuals and influence statistics are computed. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="intercept"></param>
        public static MultipleLinearRegressionResult Compute(double[,] x, List<double> y, List<double> w, Intercept intercept = Intercept.Include) {
            var weights = Stats.Normalize(w);
            var results = new MultipleLinearRegressionResult();
            using (var R = new RDotNetEngine()) {
                var n = y.Count;
                var nCol = x.GetLength(1);
                try {
                    R.SetSymbol("x", x);
                    R.SetSymbol("y", y.ToArray());
                    R.EvaluateNoReturn("y = as.numeric(y)");
                    R.SetSymbol("wt", weights.ToArray());
                    R.EvaluateNoReturn("wt = as.numeric(wt)");
                    if (intercept == Intercept.Include) {
                        R.EvaluateNoReturn("obj<-lm(y ~  0 + x, weights = wt)");
                    } else {
                        R.EvaluateNoReturn("obj<-lm(y ~ -1 + x, weights = wt)");
                    }
                    var deviance = R.EvaluateDouble("deviance(obj)");
                    var coefficients = R.EvaluateNumericVector("coefficients(obj)");
                    var standardErrors = R.EvaluateNumericVector("summary(obj)$coefficients[,2]");
                    for (int i = 0; i < coefficients.Count; i++) {
                        if (double.IsNaN(coefficients[i])) {
                            standardErrors.Insert(i, 0);
                            coefficients[i] = 0;
                        }
                    }
                    var fitted = R.EvaluateNumericVector("fitted(obj)");
                    var residuals = R.EvaluateNumericVector("residuals(obj)");
                    var degreesOfFreedom = R.EvaluateInteger("df.residual(obj)");
                    results.Sigma2 = R.EvaluateDouble("as.numeric(summary(obj)$sigma^2)");
                    results.MeanDeviance = deviance / (Convert.ToDouble(degreesOfFreedom));
                    results.RegressionCoefficients = coefficients;
                    results.StandardErrors = standardErrors;
                    results.DegreesOfFreedom = degreesOfFreedom;
                    results.FittedValues = fitted;
                    results.Residuals = residuals;
                } catch (Exception ex) {
                    if (n > 0) {
                        return new MultipleLinearRegressionResult() {
                            RegressionCoefficients = [y.Average()],
                            StandardErrors = [0],
                            DegreesOfFreedom = y.Count - 1,
                            MeanDeviance = 0,
                            FittedValues = new double[n].ToList(),
                            Sigma2 = 0,
                            Residuals = Enumerable.Repeat(0D, y.Count).ToList(),
                        };
                    } else {
                        throw new ParameterFitException(ex.Message);
                    }
                }
                return results;
            }
        }
    }
}
