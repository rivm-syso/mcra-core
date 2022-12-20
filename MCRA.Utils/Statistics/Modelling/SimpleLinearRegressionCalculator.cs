using MCRA.Utils.R.REngines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Statistics.Modelling {

    public enum Intercept {
        Omit,
        Include,
    }

    public sealed class SimpleLinearRegressionCalculator {

        /// <summary>
        /// Performs a simple linear regression with a constant term.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static SimpleLinearRegressionResult Compute(IEnumerable<double> x, IEnumerable<double> y) {
            var intercept = Intercept.Include;
            var weights = Enumerable.Repeat(1d, x.Count());
            return Compute(x, y, weights, intercept);
        }

        /// <summary>
        /// Performs a simple linear regression with or without a constant term. The data is optionally weighted.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="intercept"></param>
        /// <returns></returns>
        public static SimpleLinearRegressionResult Compute(
            IEnumerable<double> x,
            IEnumerable<double> y,
            IEnumerable<double> w,
            Intercept intercept
        ) {
            var results = new SimpleLinearRegressionResult();

            var weights = Stats.Normalize(w.ToList());
            if (x.Count() < 2) {
                throw new ParameterFitException("Too few data points for linear regression");
            }
            using (var R = new RDotNetEngine()) {
                try {
                    R.SetSymbol("x", x.ToArray());
                    R.EvaluateNoReturn("x = as.numeric(x)");
                    R.SetSymbol("y", y.ToArray());
                    R.EvaluateNoReturn("y = as.numeric(y)");
                    R.SetSymbol("wt", weights.ToArray());
                    R.EvaluateNoReturn("wt = as.numeric(wt)");
                    if (intercept == Intercept.Include) {
                        R.EvaluateNoReturn("obj<-lm(y ~ x, weights = wt)");
                    } else {
                        R.EvaluateNoReturn("obj<-lm(y ~ -1 + x, weights = wt)");
                    }
                    var regrCoef = R.EvaluateNumericVector("coef(obj)");
                    var deviance = R.EvaluateDouble("deviance(obj)");
                    var dfRes = R.EvaluateInteger("df.residual(obj)");
                    results.MeanDeviance = deviance / (Convert.ToDouble(dfRes));
                    if (intercept == Intercept.Include) {
                        results.Constant = regrCoef[0];
                        results.Coefficient = regrCoef[1];
                    } else {
                        results.Constant = 0;
                        results.Coefficient = regrCoef[0];
                    }
                } catch (Exception ex) {
                    throw new ParameterFitException(ex.Message);
                }
            }
            return results;
        }
    }
}
