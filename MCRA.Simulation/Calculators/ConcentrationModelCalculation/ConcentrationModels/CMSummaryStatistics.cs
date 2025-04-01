using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 8: Summary statistics model.
    /// </summary>
    public sealed class CMSummaryStatistics : ConcentrationModel {

        /// <summary>
        /// Log-normal model parameter mu.
        /// </summary>
        public double Mu { get; private set; }

        /// <summary>
        /// Log-normal model parameter sigma.
        /// </summary>
        public double Sigma { get; private set; }

        /// <summary>
        /// Override
        /// </summary>
        public override bool CalculateParameters() {
            if (ConcentrationDistribution != null && ConcentrationDistribution.CV != null) {
                FractionPositives = CorrectedWeightedAgriculturalUseFraction;
                FractionCensored = 0D;
                FractionNonDetects = 0D;
                FractionNonQuantifications = 0D;
                FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;
                var unitCorrection = ConcentrationDistribution.ConcentrationUnit.GetConcentrationUnitMultiplier(ConcentrationUnit);
                Sigma = Math.Log(Math.Pow((double)ConcentrationDistribution.CV, 2) + 1);
                Mu = Math.Log(unitCorrection * ConcentrationDistribution.Mean) - .5 * Math.Pow(Sigma, 2);
            } else if (Residues.Positives.Any()
                && Residues.StandardDeviation != null
                && !double.IsNaN((double)Residues.StandardDeviation)
            ) {
                FractionPositives = Residues.FractionPositives;
                FractionCensored = CorrectedWeightedAgriculturalUseFraction - FractionPositives;
                FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues;
                FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;

                FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;
                Sigma = Residues.StandardDeviation ?? 0D;
                Mu = Math.Log(Residues.Positives.Average()) - .5 * Math.Pow(Sigma, 2);
            } else {
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
            if (random.NextDouble() < CorrectedWeightedAgriculturalUseFraction) {
                return drawFromLogNormal(random);
            }
            return 0D;
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return drawFromLogNormal(random);
        }

        /// <summary>
        /// Default model for this class
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private double drawFromLogNormal(IRandom random) {
            var x = NormalDistribution.InvCDF(0, 1, random.NextDouble()) * Sigma + Mu;
            if (double.IsNaN(UtilityFunctions.ExpBound(x))) {
                throw new Exception();
            }
            return UtilityFunctions.ExpBound(x);
        }

        /// <summary>
        /// Replace censored values according to the NonDetectsHandlingMethod
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Override: returns the distribution mean
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (FractionCensored > 0 && (Residues?.CensoredValues?.Count > 0)) {
                var weightedAveragePositives = FractionPositives * UtilityFunctions.ExpBound(Mu + .5 * Math.Pow(Sigma, 2));
                var replacementFactor = nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? 1 : 0D;
                var weightedAverageCensoredValues = 0d;
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                    weightedAverageCensoredValues = FractionCensored * Residues.CensoredValues.AverageOrZero() * FractionOfLor * replacementFactor;
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                    var meanLoqLod = Residues.CensoredValuesCollection
                        .AverageOrZero(c => c.ResType == ResType.LOD ? c.LOD * FractionOfLor : c.LOD + FractionOfLor * (c.LOQ - c.LOD));
                    weightedAverageCensoredValues = FractionCensored * meanLoqLod * replacementFactor;
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem) {
                    var meanLoqLod = Residues.CensoredValuesCollection
                        .AverageOrZero(c => c.ResType == ResType.LOD ? 0 : FractionOfLor * c.LOQ);
                    weightedAverageCensoredValues = FractionCensored * meanLoqLod * replacementFactor;
                }
                return weightedAveragePositives + weightedAverageCensoredValues;
            } else {
                var residue = CorrectedWeightedAgriculturalUseFraction * UtilityFunctions.ExpBound(Mu + 0.5 * Math.Pow(Sigma, 2));
                return residue;
            }
        }

        /// <summary>
        /// Override: returns the model type (censored value spike log normal)
        /// </summary>
        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.SummaryStatistics; }
        }

        /// <summary>
        /// Draws new parameters for Parametric Uncertainty of Model 2
        /// Employs Beta distribution for Spike with Uniform prior, i.e. Beta(1,1)
        /// Employs Exact distribution for (Mu, Sigma*Sigma).
        /// </summary>
        public override void DrawParametricUncertainty(IRandom random) {
            throw new NotImplementedException();
        }

        public override bool IsParametric() {
            return false;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
