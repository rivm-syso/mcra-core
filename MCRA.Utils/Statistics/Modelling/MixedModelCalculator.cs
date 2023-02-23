using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Utils.Statistics.Modelling {

    public enum MixedModelMethod {
        REML,
        ML,
    };

    public sealed class MixedModelCalculator {
        /// <summary>
        /// Fits a Linear Mixed effects regression Model using maximum likelihood (ML).
        /// </summary>
        public static RemlResult FitMixedModel(double[,] design, List<double> y, List<int> ind, List<double> w, MixedModelMethod method = MixedModelMethod.ML) {
            var result = new RemlResult();
            var weights = Stats.Normalize(w);

            var dim = 1;
            if (design != null) {
                dim = design.GetLength(1);
            }

            using (var R = new RDotNetEngine()) {
                R.LoadLibrary("lme4");
                try {
                    R.SetSymbol("y", y.ToArray());
                    R.EvaluateNoReturn("y=as.numeric(y)");
                    R.SetSymbol("ind", ind.ToArray());
                    R.EvaluateNoReturn("individual<-as.factor(ind)");
                    R.SetSymbol("w", weights.ToArray());
                    R.EvaluateNoReturn("w=as.numeric(w)");

                    var fixedModel = "1";
                    if (design != null) {
                        R.SetSymbol("x", design);
                        fixedModel += " + x";
                    }

                    // LMER gives wrong variance components for weights unequal to 1. See also covariatemodelling.xlsx (Waldo)
                    if (method == MixedModelMethod.ML) {
                        R.EvaluateNoReturn("obj<-lmer(y~ " + fixedModel + " + (1|individual),  weights = w,  REML = FALSE)");
                    } else {
                        R.EvaluateNoReturn("obj<-lmer(y~ " + fixedModel + " + (1|individual),  weights = w,  REML = TRUE)");
                    }

                    result.VarianceWithin = Math.Pow(R.EvaluateDouble("as.numeric(attr(VarCorr(obj), 'sc'))"), 2);
                    result.VarianceBetween = R.EvaluateDouble("as.numeric(VarCorr(obj)$individual)");

                    //warning = "no conv.";
                    var b = new double[dim];
                    var vc = new double[dim * dim];
                    var se = new List<double>();
                    if (design != null) {
                        b = R.EvaluateNumericVector("as.numeric(fixef(obj))").ToArray();
                        vc = R.EvaluateNumericVector("as.numeric(vcov(obj, useScale = FALSE))").ToArray();
                        dim = b.Length;
                    } else {
                        b[0] = R.EvaluateDouble("as.numeric(fixef(obj))");
                        vc[0] = R.EvaluateDouble("as.numeric(vcov(obj, useScale = FALSE))");
                    }
                    for (int i = 0; i < dim * dim; i++) {
                        se.Add(Math.Sqrt(vc[i]));
                        i += dim;
                    }

                    // Residuals contains error of lowest stratum
                    var residuals = R.EvaluateNumericVector("as.numeric(resid(obj))");

                    // Fitted contains fixed effects and random (blup) effects
                    var fitted = R.EvaluateNumericVector("as.numeric(fitted(obj))");

                    // FitFixed contains fixed effects only
                    var fitFixed = new double[residuals.Count];

                    // Blups contains random effects
                    // var ranEf = R.Evaluate("as.numeric(ranef(obj)$individual[,1])") as double[];
                    var blups = new List<double>();

                    // Error contains error of lowest stratum and randomeffects
                    var errors = new List<double>();
                    if (design != null) {
                        for (int i = 0; i < residuals.Count; i++) {
                            fitFixed[i] = b[0];
                            for (int j = 0; j < design.GetLength(1); j++) {
                                fitFixed[i] += design[i, j] * b[j + 1];
                            }
                        }
                    } else {
                        for (int i = 0; i < residuals.Count; i++) {
                            fitFixed[i] = b[0];
                        }
                    }

                    for (int i = 0; i < residuals.Count; i++) {
                        var blup = fitted[i] - fitFixed[i];
                        blups.Add(blup);
                        errors.Add(residuals[i] + blup);
                    }

                    result.Estimates = b.ToList();
                    result.Se = se;
                    result._2LogLikelihood = -2.0 * R.EvaluateDouble("as.numeric(logLik(obj))");
                    result.Df = y.Count - 2 - b.Length;
                    result.Blups = blups;
                    result.Residuals = errors;
                    result.FittedValues = fitted.ToList();
                } catch {
                    var n = y.Count;
                    var constant = y.Count > 0 ? y.Average() : double.MinValue;
                    result.VarianceBetween = 0;
                    result.VarianceWithin = 0;
                    result.Estimates = Enumerable.Repeat(constant, dim).ToList();
                    result.Se = Enumerable.Repeat(0D, dim).ToList();
                    result.Df = y.Count - 2 - dim;
                    result._2LogLikelihood = 0;
                    result.FittedValues = Enumerable.Repeat(constant, n).ToList();
                    result.Blups = Enumerable.Repeat(0D, n).ToList();
                    result.Residuals = Enumerable.Repeat(constant, n).ToList();
                }
                return result;
            }
        }

        /// <summary>
        /// Fits a linear mixed effects regression model using maximum likelihood (ML) only for ISUF (no covariates allowed).
        /// </summary>
        /// <param name="yR">Responses</param>
        /// <param name="indR">Individual ids</param>
        /// <param name="distinctLevels"></param>
        /// <returns></returns>
        public static List<double> MLRandomModel(List<double> yR, List<int> indR, List<int> distinctLevels) {
            using (var R = new RDotNetEngine()) {
                var n = yR.Count;
                var nrf = distinctLevels.Count;
                var gamma = new double[2];
                var estAmount = new double[1];
                var seAmount = new double[1];
                var fit = new double[n];
                var tmp = new double[n];
                for (int i = 0; i < n; i++) {
                    tmp[i] = Convert.ToInt32(indR[i]);
                }
                try {
                    R.EvaluateNoReturn("library(nlme)");
                    R.SetSymbol("y", yR);
                    R.EvaluateNoReturn("y=as.numeric(y)");
                    R.SetSymbol("ind", tmp);
                    R.EvaluateNoReturn("ind=as.numeric(ind)");
                    R.EvaluateNoReturn("individual<-as.factor(ind)");
                    R.EvaluateNoReturn("obj<-lme(y~1, random=~1|individual, method ='ML')");
                    var varCorr = R.EvaluateNumericVector("as.numeric(nlme::VarCorr(obj))");
                    var tTable = R.EvaluateNumericVector("as.numeric(summary(obj)$tTable)");
                    estAmount[0] = tTable[0];
                    seAmount[0] = tTable[1];
                    gamma[0] = varCorr[0];
                    gamma[1] = varCorr[1];
                } catch (Exception ex) {
                    throw new ParameterFitException(ex.Message);
                }

                var index = 0;
                var ix = new int[n];
                for (int i = 0; i < n; i++) {
                    for (int j = 0; j < nrf; j++) {
                        if (distinctLevels[j] == indR[i]) {
                            ix[i] = j;
                            break;
                        }
                    }
                }
                var observedError = new List<double>();
                for (int i = 0; i < n; i++) {
                    index = ix[i];
                    observedError.Add(yR[i] - estAmount[0]);
                }
                return observedError;
            }
        }
    }
}
