using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Model 1: empirical distribution. Values of the spike maybe replaced by LOR.
    /// </summary>
    public sealed class CMEmpirical : ConcentrationModel {

        /// <summary>
        /// Override
        /// </summary>
        public override bool CalculateParameters() {
            FractionPositives = Residues.FractionPositives;
            FractionCensored = CorrectedWeightedAgriculturalUseFraction - FractionPositives;
            FractionNonDetects = FractionCensored * Residues.FractionNonDetectValues / Residues.FractionCensoredValues; 
            FractionNonQuantifications = FractionCensored * Residues.FractionNonQuantificationValues / Residues.FractionCensoredValues;
            FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;
            return true;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (Residues.NumberOfResidues == 0 || CorrectedWeightedAgriculturalUseFraction == 0) {
                return 0D;
            } else {
                if (random.NextDouble() < FractionPositives) {
                    if (Residues.Positives.Count > 0) {
                        var i = random.Next(Residues.Positives.Count);
                        return Residues.Positives[i];
                    }
                }
                var pDrawCensored = FractionCensored / (1 - FractionPositives);
                return DrawAccordingToNonDetectsHandlingMethod(random, nonDetectsHandlingMethod, pDrawCensored);
            }
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (CorrectedWeightedAgriculturalUseFraction == 0 || Residues.NumberOfResidues == 0) {
                return 0D;
            } else {
                var pPositive = FractionPositives / CorrectedWeightedAgriculturalUseFraction;
                if (random.NextDouble() < pPositive) {
                    var i = random.Next(Residues.Positives.Count);
                    return Residues.Positives[i];
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
                    }
                }
            }
            return 0D;
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (Residues.NumberOfResidues == 0) {
                // Check the residue count, this situation can occur in cumulative assessments
                return 0D;
            } else {
                var replacementFactor = nonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? 1 : 0D;
                var pPositive = FractionPositives;
                var pCensoredNonDetect = CorrectedWeightedAgriculturalUseFraction - pPositive;
                var weightedAverageCensoredValues = 0d;
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                    weightedAverageCensoredValues = pCensoredNonDetect * Residues.CensoredValues.AverageOrZero() * FractionOfLOR * replacementFactor;
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                    var meanLoqLod = Residues.CensoredValuesCollection
                        .AverageOrZero(c => c.ResType == ResType.LOD ? c.LOD * FractionOfLOR : c.LOD + FractionOfLOR * (c.LOQ - c.LOD));
                    weightedAverageCensoredValues = pCensoredNonDetect * meanLoqLod * replacementFactor;
                }
                var weightedAveragePositives = pPositive * Residues.Positives.AverageOrZero();
                return weightedAveragePositives + weightedAverageCensoredValues;
            }
        }

        /// <summary>
        /// Override: return the model type of this model (empirical).
        /// </summary>
        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.Empirical; }
        }

        /// <summary>
        /// Override: does not apply for empirical concentration models.
        /// </summary>
        /// <param name="random"></param>
        public override void DrawParametricUncertainty(IRandom random) {
            // No Parametric Uncertainty for emperical model
            throw new NotImplementedException();
        }

        public override bool IsParametric() {
            return false;
        }

        /// <summary>
        /// Returns an imputation value for the censored substance concentration.
        /// </summary>
        /// <param name="sampleSubstance"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            return GetDeterministicImputationValue(sampleSubstance, NonDetectsHandlingMethod, FractionOfLOR);
        }
    }
}
