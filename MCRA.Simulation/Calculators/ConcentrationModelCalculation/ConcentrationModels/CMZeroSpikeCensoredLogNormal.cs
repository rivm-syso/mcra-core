using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 6: mixture model, spike true zeros en censored lognormal, No replacement of censored values.
    /// </summary>
    public sealed class CMZeroSpikeCensoredLogNormal : ConcentrationModel {
        public override ConcentrationModelType ModelType => ConcentrationModelType.ZeroSpikeCensoredLogNormal;

        public double Mu { get; private set; }

        public double Sigma { get; private set; }

        /// <summary>
        /// Variance-Covariance matrix for parameters (mu, Log(sigma*sigma), Logit(p)); for Parametric Uncertainty
        /// </summary>
        private GeneralMatrix _vcov;

        /// <summary>
        /// Estimates of parameters (mu, Log(sigma*sigma), Logit(p)) collected in an Array; for Parametric Uncertainty
        /// </summary>
        private double[] _estimates;

        /// <summary>
        /// Choleski decomposition of Variance-Covariance matrix for parameters (mu, Log(sigma*sigma), Logit(p)); for Parametric Uncertainty
        /// </summary>
        private double[,] _vcovChol;

        /// <summary>
        /// Override
        /// </summary>
        public override bool CalculateParameters() {
            if (Residues == null
                || Residues.CensoredValues.Count == 0
                || Residues.Positives.Count == 0
                || Residues.Positives.Max() == Residues.Positives.Min()
            ) {
                return false;
            }
            try {
                var logResidues = Residues.Positives.Select(c => Math.Log(c)).ToList();
                var logCensoredValues = Residues.CensoredValues.Select(c => Math.Log(c)).ToList();
                (Mu, Sigma, FractionTrueZeros) = fitMixtureZeroSpikeCensoredLogNormal(logResidues, logCensoredValues);
                FractionPositives = Residues.FractionPositives;
                FractionCensored = (1 - FractionTrueZeros) - FractionPositives;
                FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
                FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
                // For parametric Bootstrap
                PrepareParametricUncertainty(logResidues, logCensoredValues, Mu, Sigma, FractionTrueZeros);
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var p = FractionCensored + FractionPositives;
            if (random.NextDouble() < p) {
                return UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu);
            }
            return 0D;
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive).
        /// </summary>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return DrawFromDistribution(random, nonDetectsHandlingMethod);
        }

        /// <summary>
        /// Replace nondetects according to the censored values handling method.
        /// </summary>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double p) {
            return 0D;
        }

        /// <summary>
        /// Override: returns the distribution mean
        /// </summary>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return FractionPositives * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
        }

        public override bool IsParametric => true;

        /// <summary>
        /// Draws new parameters for Parametric Bootstrap.
        /// Employs Large-Sample Multivariate Normality with Variance-Covariance matrix of the MLEs of  (mu, Log(sigma*sigma), Logit(p))
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            var distribution = new MultiVariateNormalDistribution(_estimates.ToList(), _vcovChol);
            var draw = distribution.Draw(random);
            Mu = draw[0];
            Sigma = Math.Sqrt(Math.Exp(draw[1]));
            CorrectedOccurenceFraction = UtilityFunctions.ILogit(draw[2]);
            if (double.IsNaN(Mu)) {
                Mu = _estimates[0];
            }
            if (double.IsNaN(Sigma) || double.IsInfinity(Sigma)) {
                Sigma = 0;
            }
        }

        /// <summary>
        /// Model 6: Mixture of zero spike and Censored Normal (left censoring only). Optimization in terms of mu, Log(sigma*sigma) and Logit(p0)
        /// </summary>
        /// <param name="positives">Positive concentrations</param>
        /// <param name="censoredValues">Non detects</param>
        /// <param name="method">Optimization method; see R function optim.</param>
        /// <remarks>Implemented by Paul Goedhart</remarks>
        public (double mu, double sigma, double zeroSpike) fitMixtureZeroSpikeCensoredLogNormal(List<double> positives, List<double> censoredValues) {
            if ((positives == null) || (positives.Count < 2)) {
                throw new ParameterFitException("Unable to fit MixtureZeroSpikeCensoredLogNormal because there are less than two positive values.");
            } else if ((censoredValues == null) || (censoredValues.Count < 1)) {
                throw new ParameterFitException("Unable to fit MixtureZeroSpikeCensoredLogNormal because there are no censored values.");
            } else if (positives.Max() == positives.Min()) {
                throw new ParameterFitException("Unable to fit MixtureZeroSpikeCensoredLogNormal because there is no measured variance.");
            }

            // Define limits for tau = Log(sigma*sigma)  and for eta = Logit(p0)
            var lowerTau = -20D;
            var upperTau = 20D;
            var smallestP0 = 1.0e-6;
            var lowerEta = UtilityFunctions.Logit(smallestP0 / 2D);
            var upperEta = UtilityFunctions.ILogit(1 - smallestP0 / 2d);

            // Initial values
            var mu = positives.Average();
            var sigma2 = positives.Variance();
            double sigma;
            var tau = 0D;
            if ((double.IsNaN(sigma2) == false) && (sigma2 > 0)) {
                tau = Math.Log(sigma2);
            }
            var zeroSpike = Convert.ToDouble(censoredValues.Count) / Convert.ToDouble(censoredValues.Count + positives.Count) / 2D;
            var eta = UtilityFunctions.Logit(zeroSpike);
            var ini = new double[] { mu, tau, eta };

            // Use R to fit the model
            using (var R = new RDotNetEngine()) {
                try {
                    R.SetSymbol("ini", ini);
                    R.SetSymbol("positives", positives.ToArray());
                    R.SetSymbol("nondetects", censoredValues.ToArray());
                    R.SetSymbol("lowerTau", lowerTau);
                    R.SetSymbol("upperTau", upperTau);
                    R.SetSymbol("lowerEta", lowerEta);
                    R.SetSymbol("upperEta", upperEta);
                    R.EvaluateNoReturn("ini = as.numeric(ini)");
                    R.EvaluateNoReturn("positives = as.numeric(positives)");
                    R.EvaluateNoReturn("nondetects = as.numeric(nondetects)");
                    var model = "deviance = function(parameters, positives, nondetects, lowerTau, upperTau, lowerEta, upperEta) { " +
                                "  mu = parameters[1];" +
                                "  if (parameters[2] > upperTau) {" +
                                "      sigma = sqrt(exp(upperTau));" +
                                "    } else if (parameters[2] < lowerTau) {" +
                                "      sigma = sqrt(exp(lowerTau));" +
                                "    } else { " +
                                "      sigma = sqrt(exp(parameters[2]));" +
                                "    };" +
                                "  if (parameters[3] > upperEta) {" +
                                "      p0 = 1/(1+exp(-upperEta));" +
                                "    } else if (parameters[3] < lowerEta) {" +
                                "      p0 = 1/(1+exp(-lowerEta));" +
                                "    } else {" +
                                "      p0 = 1/(1+exp(-parameters[3]));" +
                                "    };" +
                                "  likCens  = sum(log(p0 + (1-p0)*pnorm(nondetects,mu,sigma)));" +
                                "  likNon   = sum(log((1-p0)*dnorm(positives,mu,sigma)));" +
                                "  return(-2*(likCens + likNon))" +
                                "}";
                    var fitmodel = "fitmodel = optim(ini, method=\"Nelder-Mead\", fn=deviance, positives=positives, nondetects=nondetects, " +
                                    "           lowerTau=lowerTau, upperTau=upperTau, lowerEta=lowerEta, upperEta=upperEta);";
                    R.EvaluateNoReturn(model);
                    R.EvaluateNoReturn(fitmodel);
                    var deviance = R.EvaluateDouble("fitmodel$value");
                    mu = R.EvaluateDouble("fitmodel$par[1]");
                    tau = R.EvaluateDouble("fitmodel$par[2]");
                    eta = R.EvaluateDouble("fitmodel$par[3]");
                    sigma = Math.Sqrt(Math.Exp(tau));
                    zeroSpike = UtilityFunctions.ILogit(eta);
                } catch (Exception e) {
                    throw new ParameterFitException("Error in ZeroSpikeCensoredLogNormal module: " + e.Message);
                }
                if (zeroSpike <= smallestP0) {
                    throw new ParameterFitException("Estimate of spike probability in MixtureZeroSpikeCensoredLogNormal is smaller than its lower bound " + smallestP0 + ". " +
                        "Revert to Censored LogNormal.");
                }
                var fractionNonDetects = censoredValues.Count / (double)(censoredValues.Count + positives.Count);
                if (zeroSpike > fractionNonDetects) {
                    throw new ParameterFitException("Estimate of spike probability in MixtureZeroSpikeCensoredLogNormal is greater than the fraction of censored values. " +
                        "Revert to Censored LogNormal.");
                }
            }
            return (mu, sigma, zeroSpike);
        }

        /// <summary>
        /// Prepare for Parametric Uncertainty
        /// Calculates the Large-Sample Variance-Covariance matrix for MLE of (mu, Log(sigma*sigma), Logit(p))
        /// Only public to accomodate Unit Testing in ConcentrationModelling Test
        /// </summary>
        public void PrepareParametricUncertainty(List<double> pos, List<double> lors, double mu, double sigma, double spike) {
            // Set estimates
            var tau = Math.Log(sigma * sigma);
            var logitp = UtilityFunctions.Logit(spike);
            _estimates = [mu, tau, logitp];
            // Derivatives that do not depend on data
            var SigmaR = 1D / sigma;
            var SigmaR2 = SigmaR * SigmaR;
            var Spike1 = 1D - spike;
            var dmu = -SigmaR;
            var dmixed = 0.5 * SigmaR;
            var deta = spike * Spike1;
            var deta2 = deta * (1D - 2D * spike);
            // Initialize elements of Vcov
            var d11 = 0D;
            var d22 = 0D;
            var d33 = 0D;
            var d12 = 0D;
            var d13 = 0D;
            var d23 = 0D;
            foreach (var ipos in pos) { // Non-Censored observation
                var dres = (ipos - mu);
                d11 += -SigmaR2;
                d22 += -0.5 * dres * dres * SigmaR2;
                d33 += -deta;
                d12 += -dres * SigmaR2;
            }
            foreach (var ilors in lors) { // Censored observation
                var dLor = (ilors - mu) * SigmaR;
                var PHI = NormalDistribution.CDF(0, 1, dLor);
                var phi = UtilityFunctions.PRNormal(dLor);
                var tmp = spike + Spike1 * PHI;
                var quot1 = Spike1 * phi / tmp;
                var quot2 = Spike1 * phi * (-dLor * tmp - Spike1 * phi) / (tmp * tmp);
                var quot3 = -phi * (tmp + Spike1 * (1D - PHI)) / (tmp * tmp);
                var quot4 = (1D - PHI) / tmp;
                // derivatives of F
                var dtau = -0.5 * dLor;
                var dtau2 = 0.25 * dLor;
                // Contributions to second-order derivatives
                d11 += quot2 * dmu * dmu;
                d22 += quot2 * dtau * dtau + quot1 * dtau2;
                d33 += quot4 * (-quot4 * deta * deta + deta2);
                d12 += quot2 * dmu * dtau + quot1 * dmixed;
                d13 += quot3 * dmu * deta;
                d23 += quot3 * dtau * deta;
            }
            _vcov = new GeneralMatrix(3, 3);
            _vcov.SetElement(0, 0, -d11);
            _vcov.SetElement(0, 1, -d12);
            _vcov.SetElement(0, 2, -d13);
            _vcov.SetElement(1, 0, -d12);
            _vcov.SetElement(1, 1, -d22);
            _vcov.SetElement(1, 2, -d23);
            _vcov.SetElement(2, 0, -d13);
            _vcov.SetElement(2, 1, -d23);
            _vcov.SetElement(2, 2, -d33);
            _vcov = _vcov.Inverse();
            _vcovChol = _vcov.ArrayCopy2;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
