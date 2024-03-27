using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 2: Nondetect spike and Lognormal distribution. Values of the spike maybe replaced by LOR.
    /// </summary>
    public sealed class CMNonDetectSpikeLogNormal : ConcentrationModel {

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
                    || !Residues.Positives.Any()
                    || Residues.Positives.Max() == Residues.Positives.Min()
                ) {
                    return false;
                }

                FractionPositives = Residues.FractionPositives;
                FractionCensored = CorrectedWeightedAgriculturalUseFraction - FractionPositives;
                FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
                FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
                FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;

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
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedWeightedAgriculturalUseFraction == 0) {
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
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedWeightedAgriculturalUseFraction == 0) {
                return 0D;
            } else {
                var pPositive = FractionPositives / CorrectedWeightedAgriculturalUseFraction;
                if (random.NextDouble() < pPositive) {
                    return DrawFromLogNormal(random);
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
            if (Residues.CensoredValues.Count > 0) {
                var iLor = random.Next(Residues.CensoredValues.Count);
                if (random.NextDouble() < fraction && nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                    var resType = Residues.CensoredValuesCollection[iLor].ResType;
                    if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                        return Residues.CensoredValues[iLor] * FractionOfLOR;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && resType == ResType.LOD) {
                        return Residues.CensoredValuesCollection[iLor].LOD * FractionOfLOR;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && resType == ResType.LOQ) {
                        return Residues.CensoredValuesCollection[iLor].LOD + FractionOfLOR * (Residues.CensoredValuesCollection[iLor].LOQ - Residues.CensoredValuesCollection[iLor].LOD);
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && resType == ResType.LOD) {
                        return 0;
                    } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && resType == ResType.LOQ) {
                        return FractionOfLOR * Residues.CensoredValuesCollection[iLor].LOQ;
                    }
                }
            }
            return 0D;
        }


        /// <summary>
        /// Default model for this class
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
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
        /// <param name="random"></param>
        /// <param name="lor"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
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
        /// Override: returns the distribution mean
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="agriculturalUseFractionAndLor"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var replacementFactor = nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? 1 : 0D;
            var pPositive = (Residues.NumberOfResidues > 0) ? FractionPositives : CorrectedWeightedAgriculturalUseFraction;
            var pCensoredNonDetect = CorrectedWeightedAgriculturalUseFraction - pPositive;
            var weightedAverageCensoredValues = 0d;
            if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                weightedAverageCensoredValues = pCensoredNonDetect * Residues.CensoredValues.AverageOrZero() * FractionOfLOR * replacementFactor;
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                var meanLoqLod = Residues.CensoredValuesCollection
                    .AverageOrZero(c => c.ResType == ResType.LOD ? c.LOD * FractionOfLOR : c.LOD + FractionOfLOR * (c.LOQ - c.LOD));
                weightedAverageCensoredValues = pCensoredNonDetect * meanLoqLod * replacementFactor;
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem) {
                var meanLoqLod = Residues.CensoredValuesCollection
                    .AverageOrZero(c => c.ResType == ResType.LOD ? 0 : FractionOfLOR * c.LOQ);
                weightedAverageCensoredValues = pCensoredNonDetect * meanLoqLod * replacementFactor;
            }
            var weightedAveragePositives = pPositive * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
            return weightedAveragePositives + weightedAverageCensoredValues;
        }

        /// <summary>
        /// Override: returns the model type (censored value spike log normal)
        /// </summary>
        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.NonDetectSpikeLogNormal; }
        }

        /// <summary>
        /// Prepare for Parametric Uncertainty
        /// </summary>
        private void prepareParametricUncertainty(double mu, double sigma) {
            _estimates = new double[] { mu, sigma };
        }

        /// <summary>
        /// Draws new parameters for Parametric Uncertainty of Model 2
        /// Employs Beta distribution for Spike with Uniform prior, i.e. Beta(1,1)
        /// Employs Exact distribution for (Mu, Sigma*Sigma).
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            var alfa = 1D;
            var beta = 1D;
            var betaDistribution = new BetaDistribution(alfa + Residues.CensoredValues.Count, beta + Residues.NumberOfResidues - Residues.CensoredValues.Count, true);
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

        public override bool IsParametric() {
            return true;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
