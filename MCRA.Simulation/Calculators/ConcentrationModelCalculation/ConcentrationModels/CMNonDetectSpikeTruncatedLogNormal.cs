using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 3: Nondetect spike and Truncated LogNormal distribution. Values of the spike maybe replaced by LOR
    /// </summary>
    public sealed class CMNonDetectSpikeTruncatedLogNormal : ConcentrationModel {

        public double Mu { get; private set; }

        public double Sigma { get; private set; }

        private double _lor;

        /// <summary>
        /// Estimates of parameters (Mu, Log(Sigma*Sigma)) collected in an Array; for Parametric Uncertainty
        /// </summary>
        private double[] _estimates;

        /// <summary>
        /// Variance-Covariance matrix for parameters (Mu, Log(Sigma*Sigma)); for Parametric Uncertainty
        /// </summary>
        public GeneralMatrix Vcov { get; set; }

        /// <summary>
        /// Choleski decomposition of Variance-Covariance matrix for parameters (Mu, Log(Sigma*Sigma)); for Parametric Uncertainty
        /// </summary>
        private double[,] _vcovChol;

        public override bool CalculateParameters() {
            if (Residues == null
                || !Residues.CensoredValues.Any()
                || !Residues.Positives.Any()
                || Residues.Positives.Max() == Residues.Positives.Min()
            ) {
                return false;
            }
            try {
                FractionPositives = Residues.FractionPositives;
                FractionCensored = CorrectedOccurenceFraction - FractionPositives;
                FractionTrueZeros = 1 - CorrectedOccurenceFraction;

                _lor = Math.Log(Residues.CensoredValues.Max());
                var logResidues = Residues.Positives.Select(c => Math.Log(c)).ToList();
                (Mu, Sigma) = fitNonDetectSpikeTruncatedLogNormal(logResidues, _lor);
                if (double.IsNaN(Sigma) || Sigma <= 0) {
                    return false;
                }
                PrepareParametricUncertainty(logResidues, _lor, Mu, Sigma);
            } catch {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive).
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedOccurenceFraction == 0) {
                return 0D;
            } else {
                var pPositive = (Residues.NumberOfResidues > 0) ? FractionPositives : CorrectedOccurenceFraction;
                if (random.NextDouble() < pPositive) {
                    return DrawFromTruncatedLogNormal(random, _lor);
                }
                var pCensoredNonDetect = (CorrectedOccurenceFraction - pPositive) / (1 - pPositive);
                return DrawAccordingToNonDetectsHandlingMethod(random, nonDetectsHandlingMethod, pCensoredNonDetect);
            }
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedOccurenceFraction == 0) {
                return 0D;
            } else {
                var pPositive = FractionPositives / CorrectedOccurenceFraction;
                if (random.NextDouble() < pPositive) {
                    return DrawFromTruncatedLogNormal(random, _lor);
                }
                return DrawAccordingToNonDetectsHandlingMethod(random, nonDetectsHandlingMethod, 1);
            }
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            if (Residues.CensoredValues.Any()) {
                var iLor = random.Next(Residues.CensoredValues.Count);
                if (random.NextDouble() < fraction && nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                    var resType = Residues.CensoredValuesCollection[iLor].ResType;
                    if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                        return Residues.CensoredValues[iLor] * FractionOfLor;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && resType == ResType.LOD) {
                        return Residues.CensoredValuesCollection[iLor].LOD * FractionOfLor;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && resType == ResType.LOQ) {
                        return Residues.CensoredValuesCollection[iLor].LOD + FractionOfLor * (Residues.CensoredValuesCollection[iLor].LOQ - Residues.CensoredValuesCollection[iLor].LOD);
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && resType == ResType.LOD) {
                        return 0;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && resType == ResType.LOQ) {
                        return FractionOfLor * Residues.CensoredValuesCollection[iLor].LOQ;
                    }
                }
            }
            return 0D;
        }

        /// <summary>
        /// Override: returns the distribution mean
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var replacementFactor = nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? 1 : 0D;
            var pPositive = (Residues.NumberOfResidues > 0) ? FractionPositives : CorrectedOccurenceFraction;
            var pCensoredNonDetect = (CorrectedOccurenceFraction - pPositive);
            var weightedAverageNonDetects = 0d;
            if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                weightedAverageNonDetects = pCensoredNonDetect * Residues.CensoredValues.AverageOrZero() * FractionOfLor * replacementFactor;
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                var meanLoqLod = Residues.CensoredValuesCollection
                    .AverageOrZero(c => c.ResType == ResType.LOD ? c.LOD * FractionOfLor : c.LOD + FractionOfLor * (c.LOQ - c.LOD));
                weightedAverageNonDetects = pCensoredNonDetect * meanLoqLod * replacementFactor;
            }
            var weightedAveragePositives = pPositive * UtilityFunctions.ExpBound(SpecialFunctions.MeanLeftTruncatedNormal(_lor, Mu, Sigma));
            return weightedAveragePositives + weightedAverageNonDetects;
        }

        /// <summary>
        /// Draw from truncated lognormal
        /// </summary>
        /// <param name="random"></param>
        /// <param name="lor"></param>
        /// <returns></returns>
        public double DrawFromTruncatedLogNormal(IRandom random, double lor) {
            var p = NormalDistribution.CDF(Mu, Sigma, lor);
            var x = NormalDistribution.InvCDF(0, 1, random.NextDouble(p, 1)) * Sigma + Mu;
            if (double.IsNaN(UtilityFunctions.ExpBound(x))) {
                throw new Exception();
            }
            return UtilityFunctions.ExpBound(x);
        }

        /// <summary>
        /// Returns the model type, i.e., NonDetectSpikeTruncatedLogNormal
        /// </summary>
        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.NonDetectSpikeTruncatedLogNormal; }
        }

        public override bool IsParametric => true;

        /// <summary>
        /// Draws new parameters for Parametric Uncertainty for Model 3.
        /// Employs Beta distribution for Spike with Uniform prior, i.e. Beta(1,1)
        /// Employs Large-Sample Multivariate Normality with Variance-Covariance matrix of the MLEs of (Mu, Log(Sigma*Sigma)).
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            // Emperical Bayes for p
            var alfa = 1D;
            var beta = 1D;
            var betaDistribution = new BetaDistribution(alfa + Residues.CensoredValues.Count, beta + Residues.NumberOfResidues - Residues.CensoredValues.Count);
            FractionPositives = 1 - betaDistribution.Draw(random);
            //replaced by Troschuetz distribution
            //FractionPositives = 1 - RMath.rbeta(alfa + NonDetectsCount, beta + ResidueCount - NonDetectsCount);
            var draw = MultiVariateNormalDistribution.Draw(_estimates.ToList(), _vcovChol, random);
            Mu = draw[0];
            Sigma = Math.Sqrt(Math.Exp(draw[1]));

            if (double.IsNaN(Mu)) {
                Mu = _estimates[0];
            }
            if (double.IsNaN(Sigma) || double.IsInfinity(Sigma)) {
                Sigma = 0;
            }
            // MLE alternative for p. Does not work for _ySpike = 0!
            //var p = _ySpike / _nSpike;
            //var logitp = MCRA.Utils.Utils.Logit(p);
            //Spike = MCRA.Utils.RMath.rnorm(logitp, 1D / Math.Sqrt(_nSpike * p * (1D - p)));
        }

        /// <summary>
        /// Prepare for Parametric Uncertainty
        /// Calculates the Large-Sample Variance-Covariance matrix for MLE of (Mu, Log(Sigma*Sigma)).
        /// </summary>
        public void PrepareParametricUncertainty(List<double> pos, double lor, double mu, double sigma) {
            // For Spike

            // Set estimates
            var tau = Math.Log(sigma * sigma);
            _estimates = [mu, tau];
            // Prepare
            var SigmaR = 1D / sigma;
            var SigmaR2 = SigmaR * SigmaR;
            // Normal CDF and its derivatives
            var dLor = (lor - mu) * SigmaR;
            var PHI1 = 1D - NormalDistribution.CDF(0, 1, dLor);
            var phi = UtilityFunctions.PRNormal(dLor);
            var quot1 = phi / PHI1;
            var quot2 = phi * (-dLor * PHI1 + phi) / (PHI1 * PHI1);
            // derivatives of F
            var dmu = -SigmaR;
            var dtau = -0.5 * dLor;
            var dtau2 = 0.25 * dLor;
            var dmixed = 0.5 * SigmaR;
            // Initialize elements of Vcov
            var d11 = pos.Count * (-SigmaR2 + quot2 * dmu * dmu);
            var d12 = 0D;
            var d22 = 0D;
            foreach (var ipos in pos) {
                // Contributions to second-order derivatives
                var dres = (ipos - mu);
                d22 += -0.5 * dres * dres * SigmaR2 + quot2 * dtau * dtau + quot1 * dtau2;
                d12 += -dres * SigmaR2 + quot2 * dmu * dtau + quot1 * dmixed;
            }
            // Create variance-covariance matrix
            Vcov = new GeneralMatrix(2, 2);
            Vcov.SetElement(0, 0, -d11);
            Vcov.SetElement(0, 1, -d12);
            Vcov.SetElement(1, 0, -d12);
            Vcov.SetElement(1, 1, -d22);
            Vcov = Vcov.Inverse();
            // Choleski decomposition for random draws
            _vcovChol = new double[2, 2];
            _vcovChol = Vcov.chol().GetL().ArrayCopy2;
        }

        /// <summary>
        /// Model 3: Truncated Normal (left truncation only). Optimization in terms of mu and Log(sigma*sigma).
        /// </summary>
        /// <param name="positives">Positive concentrations</param>
        /// <param name="lor">Limit of reporting</param>
        /// <remarks>Implemented by Paul Goedhart</remarks>
        public (double mu, double sigma) fitNonDetectSpikeTruncatedLogNormal(List<double> positives, double lor) {
            if ((positives == null) || (positives.Count == 0)) {
                throw new ParameterFitException("Unable to fit NonDetectSpikeTrucatedLogNormal because there are no positives.");
            }

            // Define limits for tau = Log(sigma*sigma)
            var lowerTau = -20D;
            var upperTau = 20D;

            // Initial estimates
            var mu = positives.Average();
            var sigma2 = positives.Variance();
            double tau;
            double sigma;
            if ((double.IsNaN(sigma2) == false) && (sigma2 > 0)) {
                tau = Math.Log(sigma2);
            } else {
                sigma = 0D;
                return (mu, sigma);
            }

            var ini = new double[] { mu, tau };

            // Fit model using R
            using (var R = new RDotNetEngine()) {
                try {
                    R.SetSymbol("ini", ini);
                    R.SetSymbol("positives", positives.ToArray());
                    R.SetSymbol("lor", lor);
                    R.SetSymbol("lowerTau", lowerTau);
                    R.SetSymbol("upperTau", upperTau);
                    R.EvaluateNoReturn("ini = as.numeric(ini)");
                    R.EvaluateNoReturn("positives = as.numeric(positives)");
                    var model = "deviance = function(parameters, positives, lor, lowerTau, upperTau) { " +
                                "  mu = parameters[1];" +
                                "  if (parameters[2] > upperTau) {" +
                                "      sigma = sqrt(exp(upperTau));" +
                                "    } else if (parameters[2] < lowerTau) {" +
                                "      sigma = sqrt(exp(lowerTau));" +
                                "    } else { " +
                                "      sigma = sqrt(exp(parameters[2]));" +
                                "    };" +
                                "  truncProb = pnorm(lor,mu,sigma,lower.tail=FALSE);" +
                                "  return(-2*sum(log(dnorm(positives,mu,sigma)/truncProb)))" +
                                "}";
                    var fitmodel = "fitmodel = optim(ini, method=\"Nelder-Mead\", fn=deviance, positives=positives, " +
                                    "           lor=lor, lowerTau=lowerTau, upperTau=upperTau);";
                    R.EvaluateNoReturn(model);
                    R.EvaluateNoReturn(fitmodel);
                    var deviance = R.EvaluateDouble("fitmodel$value");
                    mu = R.EvaluateDouble("fitmodel$par[1]");
                    tau = R.EvaluateDouble("fitmodel$par[2]");
                    sigma = Math.Sqrt(Math.Exp(tau));
                } catch (Exception e) {
                    throw new ParameterFitException("Error in SpikeLogNormal module: " + e.Message);
                }
            }
            return (mu, sigma);
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
