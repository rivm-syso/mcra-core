using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 4: Censored LogNormal distribution. No spike, so no replacement of the nondetects. LOR is fixed.
    /// </summary>
    public sealed class CMCensoredLogNormal : ConcentrationModel {
        public override ConcentrationModelType ModelType => ConcentrationModelType.CensoredLogNormal;

        /// <summary>
        /// Estimates of parameters (Mu, Log(Sigma*Sigma)) collected in an Array; for Parametric Uncertainty
        /// </summary>
        private double[] _estimates;

        public double Mu { get; set; }

        public double Sigma { get; set; }

        /// <summary>
        /// Variance-Covariance matrix for parameters (Mu, Log(Sigma*Sigma)); for Parametric Uncertainty
        /// </summary>
        public GeneralMatrix Vcov { get; set; }

        /// <summary>
        /// Choleski decomposition of Variance-Covariance matrix for parameters (Mu, Log(Sigma*Sigma)); for Parametric Uncertainty
        /// </summary>
        private double[,] VcovChol { get; set; }

        /// <summary>
        /// Calculates the model parameters
        /// </summary>
        public override bool CalculateParameters() {
            // No positives or censored values; FAIL
            if (Residues.Positives.Count == 0 || Residues.CensoredValues.Count == 0) {
                return false;
            }

            // If all censored values are assumed to be zeros, leaving us with no censored values; FAIL
            if (CorrectedOccurenceFraction <= Residues.FractionPositives) {
                return false;
            }

            FractionPositives = Residues.FractionPositives;
            FractionCensored = CorrectedOccurenceFraction - Residues.FractionPositives;
            FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
            FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
            FractionTrueZeros = 1 - CorrectedOccurenceFraction;

            try {
                var qCens = FractionCensored / Residues.FractionCensoredValues;
                var correctedNumberOfCensoredValues = Convert.ToInt32(Math.Floor(qCens * Residues.CensoredValues.Count));
                var censoredValues = Residues.CensoredValuesCollection
                    .Take(correctedNumberOfCensoredValues)
                    .ToList();

                // LODs of non-detects and LOQs of non-quantifications without LOD
                var upperBoundedNonDetects = censoredValues
                    .Where(r => r.ResType == ResType.LOD || r.ResType == ResType.LOQ && (double.IsNaN(r.LOD) || r.LOD == 0))
                    .Select(r => r.ResType == ResType.LOD ? r.LOD : r.LOQ)
                    .ToList();

                // LOQs of non-quantifications that also have a LOD > 0
                var intervalBoundedNonQuantifications = censoredValues
                    .Where(r => r.ResType == ResType.LOQ && (!double.IsNaN(r.LOD) && r.LOD > 0))
                    .Select(r => (lower: r.LOD, upper: r.LOQ))
                    .ToList();

                var logPositives = Residues.Positives.Select(c => Math.Log(c)).ToList();
                var logUpperBoundedNonDetects = upperBoundedNonDetects.Select(c => Math.Log(c)).ToList();
                var logIntervalBoundedNonQuantifications = intervalBoundedNonQuantifications
                    .Select(c => (lower: Math.Log(c.lower), upper: Math.Log(c.upper)))
                    .ToList();

                (Mu, Sigma) = fitCensoredLogNormal(
                    logPositives,
                    logUpperBoundedNonDetects,
                    logIntervalBoundedNonQuantifications
                );

                // For parametric Bootstrap
                prepareParametricUncertainty(
                    logPositives,
                    logUpperBoundedNonDetects,
                    logIntervalBoundedNonQuantifications,
                    Mu,
                    Sigma
                );
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedOccurenceFraction == 0) {
                return 0D;
            } else {
                var pPositive = CorrectedOccurenceFraction;
                if (random.NextDouble() < pPositive) {
                    return UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu);
                }
                return 0D;
            }
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu);
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws from the distribution mean
        /// </summary>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var residue = CorrectedOccurenceFraction * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
            return residue;
        }

        public override bool IsParametric => true;

        /// <summary>
        /// Draws new parameters for Parametric Bootstrap.
        /// Employs Large-Sample Multivariate Normality with Variance-Covariance matrix of the MLEs of (Mu, Log(Sigma*Sigma)).
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            var draw = MultiVariateNormalDistribution.Draw(_estimates.ToList(), VcovChol, random);
            Mu = draw[0];
            Sigma = Math.Sqrt(Math.Exp(draw[1]));
            if (double.IsNaN(Mu)) {
                Mu = _estimates[0];
            }
            if (double.IsNaN(Sigma) || double.IsInfinity(Sigma)) {
                Sigma = 0;
            }
        }

        /// <summary>
        /// Censored Log-Normal (left censoring only). Optimization in terms of mu and Log(sigma * sigma).
        /// </summary>
        private (double mu, double sigma) fitCensoredLogNormal(
            List<double> logPositives,
            List<double> logNonDetectValues,
            List<(double lower, double upper)> logNonQuantificationValues
        ) {
            if ((logPositives == null) || (logPositives.Count == 0)) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there are no positives.");
            } else if (
                (logNonDetectValues == null && logNonQuantificationValues == null)
                || (logNonDetectValues.Count == 0 && logNonQuantificationValues.Count == 0)
            ) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there are no censored values.");
            } else if (logPositives.Max() == logPositives.Min()) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there is no measured variance.");
            }
            // lowerTau and upperTau are the bounds on log(variance) of lognormal distribution
            // Define limits for tau = Log(sigma*sigma)
            var lowerTau = -20D;
            var upperTau = 20D;

            // Initial values
            var mu = logPositives.Average();
            var sigma2 = logPositives.Variance();
            var sigma = Math.Sqrt(sigma2);
            var tau = 0D;
            if ((double.IsNaN(sigma2) == false) && (sigma2 > 0)) {
                tau = Math.Log(sigma2);
            }
            var ini = new double[] { mu, tau };

            // Fit model using R
            using (var R = new RDotNetEngine()) {
                try {
                    R.SetSymbol("ini", ini);
                    R.SetSymbol("logPositives", logPositives.ToArray());
                    R.SetSymbol("logCensored", logNonDetectValues.ToArray());
                    R.SetSymbol("logIntervalUpper", logNonQuantificationValues.Select(r => r.upper).ToArray());
                    R.SetSymbol("logIntervalLower", logNonQuantificationValues.Select(r => r.lower).ToArray());
                    R.SetSymbol("lowerTau", lowerTau);
                    R.SetSymbol("upperTau", upperTau);
                    R.EvaluateNoReturn("ini <- as.numeric(ini)");
                    R.EvaluateNoReturn("logPositives <- as.numeric(logPositives)");
                    R.EvaluateNoReturn("logCensored <- as.numeric(logCensored)");
                    R.EvaluateNoReturn("logIntervalLower <- as.numeric(logIntervalLower)");
                    R.EvaluateNoReturn("logIntervalUpper <- as.numeric(logIntervalUpper)");
                    var model = "deviance = function(parameters, logPositives, logCensored, logIntervalUpper, logIntervalLower, lowerTau, upperTau) { " +
                                "  mu <- parameters[1];" +
                                "  if (parameters[2] > upperTau) {" +
                                "      sigma <- sqrt(exp(upperTau));" +
                                "    } else if (parameters[2] < lowerTau) {" +
                                "      sigma <- sqrt(exp(lowerTau));" +
                                "    } else { " +
                                "      sigma <- sqrt(exp(parameters[2]));" +
                                "    };" +
                                "  likCens <- sum(log(pnorm(logCensored,mu,sigma)));" +
                                "  likInt <- sum(log(pnorm(logIntervalUpper,mu,sigma) - pnorm(logIntervalLower,mu,sigma)));" +
                                "  likPos <- sum(log(dnorm(logPositives,mu,sigma)));" +
                                "  return(-2*(likCens + likInt + likPos))" +
                                "}";
                    var fitmodel = "fitmodel = optim(ini, method=\"Nelder-Mead\", fn=deviance, logPositives=logPositives, logCensored=logCensored, logIntervalUpper=logIntervalUpper, " +
                                "logIntervalLower=logIntervalLower, lowerTau=lowerTau, upperTau=upperTau);";
                    R.EvaluateNoReturn(model);
                    R.EvaluateNoReturn(fitmodel);
                    var deviance = R.EvaluateDouble("fitmodel$value");
                    mu = R.EvaluateDouble("fitmodel$par[1]");
                    tau = R.EvaluateDouble("fitmodel$par[2]");
                    sigma = Math.Sqrt(Math.Exp(tau));
                } catch (Exception e) {
                    throw new ParameterFitException("Error in CensoredLogNormal module: " + e.Message);
                }
            }
            return (mu, sigma);
        }

        /// <summary>
        /// Prepare for Parametric Uncertainty
        /// Calculates the Large-Sample Variance-Covariance matrix for MLE of (Mu, Log(Sigma*Sigma))
        /// Only public to accomodate Unit Testing in ConcentrationModelling Test
        /// </summary>
        private void prepareParametricUncertainty(
            List<double> logPositives,
            List<double> logUpperBoundedNonDetects,
            List<(double lower, double upper)> logIntervalBoundedNonQuantifications,
            double mu,
            double sigma
        ) {
            // Set estimates
            var tau = Math.Log(sigma * sigma);
            _estimates = [mu, tau];
            // Derivatives that do not depend on data
            var sigmaR = 1D / sigma;
            var sigmaR2 = sigmaR * sigmaR;
            var dmu = -sigmaR;
            var dmixed = 0.5 * sigmaR;
            // Initialize elements of Vcov
            var d11 = 0D;
            var d12 = 0D;
            var d22 = 0D;
            foreach (var ipos in logPositives) {
                // Non-Censored observation
                var dres = (ipos - mu);
                d11 += -sigmaR2;
                d22 += -0.5 * dres * dres * sigmaR2;
                d12 += -dres * sigmaR2;
            }
            foreach (var ilors in logUpperBoundedNonDetects) {
                // Non-detects with only upper limit (non-detects and non-quantifications without LOD)
                // Normal CDF and its derivatives
                var dLor = (ilors - mu) * sigmaR;
                var PHI = NormalDistribution.CDF(0, 1, dLor);
                var phi = UtilityFunctions.PRNormal(dLor);
                var quot1 = phi / PHI;
                var quot2 = phi * (-dLor * PHI - phi) / (PHI * PHI);
                // derivatives of F
                var dtau = -0.5 * dLor;
                var dtau2 = 0.25 * dLor;
                // Contributions to second-order derivatives
                d11 += quot2 * dmu * dmu;
                d22 += quot2 * dtau * dtau + quot1 * dtau2;
                d12 += quot2 * dmu * dtau + quot1 * dmixed;
            }
            foreach (var ilors in logIntervalBoundedNonQuantifications) {
                // Non-quantifications between LOD and LOQ
                // Normal CDF and its derivatives
                // TODO: the code below only accounts for the upper (not lower limit)
                var dLor = (ilors.upper - mu) * sigmaR;
                var PHI = NormalDistribution.CDF(0, 1, dLor);
                var phi = UtilityFunctions.PRNormal(dLor);
                var quot1 = phi / PHI;
                var quot2 = phi * (-dLor * PHI - phi) / (PHI * PHI);
                // derivatives of F
                var dtau = -0.5 * dLor;
                var dtau2 = 0.25 * dLor;
                // Contributions to second-order derivatives
                d11 += quot2 * dmu * dmu;
                d22 += quot2 * dtau * dtau + quot1 * dtau2;
                d12 += quot2 * dmu * dtau + quot1 * dmixed;
            }
            Vcov = new GeneralMatrix(2, 2);
            Vcov.SetElement(0, 0, -d11);
            Vcov.SetElement(0, 1, -d12);
            Vcov.SetElement(1, 0, -d12);
            Vcov.SetElement(1, 1, -d22);
            Vcov = Vcov.Inverse();
            VcovChol = new double[2, 2];
            VcovChol = Vcov.chol().GetL().ArrayCopy2;
        }

        /// <summary>
        /// Draw censored imputation value for the specified sample substance.
        /// For nondetects (lod) sample from left tail (below lod)
        /// For nonquantifications (loq) sample from intermediate segment (above lod and below loq)
        /// For lors, sample from left tail (below lor)
        /// See MCRA.Utils.Test\UnitTests\Charting\Oxyplot: HistogramChartCreator_TestCreateSegmentTail and HistogramChartCreator_TestCreateLeftTail
        /// </summary>
        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            var lor = sampleSubstance.Lor;
            var loq = sampleSubstance.Loq;
            var lod = sampleSubstance.Lod;
            double draw;
            if (sampleSubstance.IsNonDetect) {
                var pLod = LogNormalDistribution.CDF(Mu, Sigma, lod);
                draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLod * random.NextDouble());
            } else if (sampleSubstance.IsNonQuantification) {
                if (double.IsNaN(lod)) {
                    var pLoq = LogNormalDistribution.CDF(Mu, Sigma, loq);
                    draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLoq * random.NextDouble());
                } else {
                    var pLod = LogNormalDistribution.CDF(Mu, Sigma, lod);
                    var pLoq = LogNormalDistribution.CDF(Mu, Sigma, loq);
                    var minValue = pLod / pLoq;
                    draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLoq * random.NextDouble(minValue, 1d));
                }
            } else {
                var pLor = LogNormalDistribution.CDF(Mu, Sigma, lor);
                draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLor * random.NextDouble());
            }
            return draw;
        }
    }
}
