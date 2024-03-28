using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 4: Censored LogNormal distribution. No spike, so no replacement of the nondetects. LOR is fixed.
    /// </summary>
    public sealed class CMCensoredLogNormal : ConcentrationModel {

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
            if (!Residues.Positives.Any() || !Residues.CensoredValues.Any()) {
                return false;
            }

            // If all censored values are assumed to be zeros, leaving us with no censored values; FAIL
            if (CorrectedWeightedAgriculturalUseFraction <= Residues.FractionPositives) {
                return false;
            }

            FractionPositives = Residues.FractionPositives;
            FractionCensored = CorrectedWeightedAgriculturalUseFraction - Residues.FractionPositives;
            FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
            FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
            FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;

            try {
                var q = FractionCensored / Residues.FractionCensoredValues;
                var correctedNumberOfCensoredValues = Convert.ToInt32(Math.Floor(q * Residues.CensoredValues.Count));
                var censoredValues = Residues.CensoredValues.Take(correctedNumberOfCensoredValues).ToList();
                var logPositives = Residues.Positives.Select(c => Math.Log(c)).ToList();
                var logCensoredValues = censoredValues.Select(c => Math.Log(c)).ToList();
                (Mu, Sigma) = fitCensoredLogNormal(logPositives, logCensoredValues);
                // For parametric Bootstrap
                prepareParametricUncertainty(logPositives, logCensoredValues, Mu, Sigma);
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedWeightedAgriculturalUseFraction == 0) {
                return 0D;
            } else {
                var pPositive = CorrectedWeightedAgriculturalUseFraction;
                if (random.NextDouble() < pPositive) {
                    return UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu);
                }
                return 0D;
            }
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu);
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws from the distribution mean
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var residue = CorrectedWeightedAgriculturalUseFraction * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
            return residue;
        }

        /// <summary>
        /// Returns the model type: in this case CensoredLogNormal
        /// </summary>
        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.CensoredLogNormal; }
        }

        public override bool IsParametric() {
            return true;
        }

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
        /// <param name="logPositives">Positive concentrations</param>
        /// <param name="logCensoredValues">Non detects</param>
        /// <remarks>Implemented by Paul Goedhart</remarks>
        public (double mu, double sigma) fitCensoredLogNormal(List<double> logPositives, List<double> logCensoredValues) {
            if ((logPositives == null) || (logPositives.Count == 0)) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there are no positives.");
            } else if ((logCensoredValues == null) || (logCensoredValues.Count == 0)) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there are no censored values.");
            } else if (logPositives.Max() == logPositives.Min()) {
                throw new ParameterFitException("Unable to fit CensoredLogNormal because there is no measured variance.");
            }

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
                    R.SetSymbol("positives", logPositives.ToArray());
                    R.SetSymbol("nondetects", logCensoredValues.ToArray());
                    R.SetSymbol("lowerTau", lowerTau);
                    R.SetSymbol("upperTau", upperTau);
                    R.EvaluateNoReturn("ini = as.numeric(ini)");
                    R.EvaluateNoReturn("positives = as.numeric(positives)");
                    R.EvaluateNoReturn("nondetects = as.numeric(nondetects)");
                    var model = "deviance = function(parameters, positives, nondetects, lowerTau, upperTau) { " +
                                "  mu = parameters[1];" +
                                "  if (parameters[2] > upperTau) {" +
                                "      sigma = sqrt(exp(upperTau));" +
                                "    } else if (parameters[2] < lowerTau) {" +
                                "      sigma = sqrt(exp(lowerTau));" +
                                "    } else { " +
                                "      sigma = sqrt(exp(parameters[2]));" +
                                "    };" +
                                "  likCens  = sum(log(pnorm(nondetects,mu,sigma)));" +
                                "  likNon   = sum(log(dnorm(positives,mu,sigma)));" +
                                "  return(-2*(likCens + likNon))" +
                                "}";
                    var fitmodel = "fitmodel = optim(ini, method=\"Nelder-Mead\", fn=deviance, positives=positives, nondetects=nondetects, " +
                                    "           lowerTau=lowerTau, upperTau=upperTau);";
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
        private void prepareParametricUncertainty(List<double> pos, List<double> lors, double mu, double sigma) {
            // Set estimates
            var tau = Math.Log(sigma * sigma);
            _estimates = new double[] { mu, tau };
            // Derivatives that do not depend on data
            var SigmaR = 1D / sigma;
            var SigmaR2 = SigmaR * SigmaR;
            var dmu = -SigmaR;
            var dmixed = 0.5 * SigmaR;
            // Initialize elements of Vcov
            var d11 = 0D;
            var d12 = 0D;
            var d22 = 0D;
            foreach (var ipos in pos) {
                // Non-Censored observation
                var dres = (ipos - mu);
                d11 += -SigmaR2;
                d22 += -0.5 * dres * dres * SigmaR2;
                d12 += -dres * SigmaR2;
            }
            foreach (var ilors in lors) {
                // Censored observation
                // Normal CDF and its derivatives
                var dLor = (ilors - mu) * SigmaR;
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
        /// <param name="sampleSubstance"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            var lor = sampleSubstance.Lor;
            var loq = sampleSubstance.Loq;
            var lod = sampleSubstance.Lod;
            double draw;
            if (sampleSubstance.IsNonDetect) {
                var pLod = LogNormalDistribution.CDF(Mu, Sigma, lod);
                draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLod * random.NextDouble());
            } else if (sampleSubstance.IsNonQuantification) {
                var pLod = LogNormalDistribution.CDF(Mu, Sigma, lod);
                var pLoq = LogNormalDistribution.CDF(Mu, Sigma, loq);
                var minValue = pLod / pLoq;
                draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLoq * random.NextDouble(minValue, 1d));
            } else {
                var pLor = LogNormalDistribution.CDF(Mu, Sigma, lor);
                draw = LogNormalDistribution.InvCDF(Mu, Sigma, pLor * random.NextDouble());
            }
            return draw;
        }
    }
}
