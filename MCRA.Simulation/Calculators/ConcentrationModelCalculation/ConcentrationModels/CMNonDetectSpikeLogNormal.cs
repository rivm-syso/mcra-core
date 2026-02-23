using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    /// <summary>
    /// Model 2: Nondetect spike and Lognormal distribution. Values of the spike maybe replaced by LOR.
    /// </summary>
    public sealed class CMNonDetectSpikeLogNormal : ConcentrationModel {
        public override ConcentrationModelType ModelType => ConcentrationModelType.NonDetectSpikeLogNormal;

        public double Mu { get; private set; }
        public double Sigma { get; private set; }

        /// <summary>
        /// Estimates of parameters (Mu, Sigma*Sigma) collected in an Array; for Parametric Uncertainty
        /// </summary>
        private double[] _estimates;

        /// <summary>
        /// Override
        /// </summary>
        public override bool CalculateParameters() {
            try {
                if (Residues?.Positives == null
                    || Residues.Positives.Count == 0
                    || Residues.Positives.Max() == Residues.Positives.Min()
                ) {
                    return false;
                }

                FractionPositives = Residues.FractionPositives;
                FractionCensored = CorrectedOccurenceFraction - FractionPositives;
                FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
                FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
                FractionTrueZeros = 1 - CorrectedOccurenceFraction;

                var logPositives = Residues.Positives.Select(p => Math.Log(p)).ToList();
                Mu = logPositives.Average();
                Sigma = Math.Sqrt(logPositives.Variance());
                prepareParametricUncertainty(Mu, Sigma);
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedOccurenceFraction == 0) {
                return 0D;
            } else {
                if (random.NextDouble() < FractionPositives) {
                    return DrawFromLogNormal(random);
                }
                var pCensoredNonDetect = FractionCensored / (1 - FractionPositives);
                return DrawAccordingToNonDetectsHandlingMethod(random, nonDetectsHandlingMethod, pCensoredNonDetect);
            }
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedOccurenceFraction == 0) {
                return 0D;
            } else {
                var pPositive = FractionPositives / CorrectedOccurenceFraction;
                if (random.NextDouble() < pPositive) {
                    return DrawFromLogNormal(random);
                }
                return DrawAccordingToNonDetectsHandlingMethod(random, nonDetectsHandlingMethod, 1);
            }
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
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
        /// Default model for this class
        /// </summary>
        private double DrawFromLogNormal(IRandom random) {
            var x = NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu;
            if (double.IsNaN(UtilityFunctions.ExpBound(x))) {
                throw new Exception();
            }
            return UtilityFunctions.ExpBound(x);
        }

        /// <summary>
        /// Draw from truncated lognormal
        /// </summary>
        public double DrawFromTruncatedLogNormal(IRandom random, double lor) {
            var p = NormalDistribution.CDF(Mu, Sigma, lor);
            var x = NormalDistribution.InvCDF(0, 1, random.NextDouble(p, 1)) * Sigma + Mu;
            if (double.IsNaN(UtilityFunctions.ExpBound(x))) {
                throw new Exception();
            }
            return UtilityFunctions.ExpBound(x);
        }

        /// <summary>
        /// Override: returns the distribution mean
        /// </summary>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var replacementFactor = nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? 1 : 0D;
            var pPositive = (Residues.NumberOfResidues > 0) ? FractionPositives : CorrectedOccurenceFraction;
            var pCensoredNonDetect = CorrectedOccurenceFraction - pPositive;
            var averageNonDetects = Residues.GetAverageNonDetects(nonDetectsHandlingMethod, FractionOfLor);
            var weightedAverageCensoredValues = pCensoredNonDetect * averageNonDetects * replacementFactor;
            var weightedAveragePositives = pPositive * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
            return weightedAveragePositives + weightedAverageCensoredValues;
        }

        /// <summary>
        /// Prepare for Parametric Uncertainty
        /// </summary>
        private void prepareParametricUncertainty(double mu, double sigma) {
            _estimates = [mu, sigma];
        }

        /// <summary>
        /// Draws new parameters for Parametric Uncertainty of Model 2
        /// Employs Beta distribution for Spike with Uniform prior, i.e. Beta(1,1)
        /// Employs Exact distribution for (Mu, Sigma*Sigma).
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            var alfa = 1D;
            var beta = 1D;
            var betaDistribution = new BetaDistribution(alfa + Residues.CensoredValues.Count, beta + Residues.NumberOfResidues - Residues.CensoredValues.Count);
            FractionPositives = 1 - betaDistribution.Draw(random);

            var chiSquareDistribution = new ChiSquaredDistribution(Residues.Positives.Count - 1, false);
            Sigma = _estimates[1] * Math.Sqrt((Residues.Positives.Count - 1) / chiSquareDistribution.Draw(random));

            Mu = NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma / Math.Sqrt(Residues.Positives.Count) + _estimates[0];
            if (double.IsNaN(Mu)) {
                Mu = _estimates[0];
            }
            if (double.IsNaN(Sigma) || double.IsInfinity(Sigma)) {
                Sigma = 0;
            }
        }
        public override bool IsParametric => true;

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
