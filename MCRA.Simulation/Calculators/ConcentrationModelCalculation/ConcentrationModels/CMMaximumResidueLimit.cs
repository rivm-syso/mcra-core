using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    public sealed class CMMaximumResidueLimit : ConcentrationModel {

        public override ConcentrationModelType ModelType {
            get { return ConcentrationModelType.MaximumResidueLimit; }
        }

        /// <summary>
        /// Override: computes the model parameters
        /// </summary>
        public override bool CalculateParameters() {
            FractionPositives = CorrectedWeightedAgriculturalUseFraction;
            FractionCensored = 0D;
            FractionNonDetects = 0D;
            FractionNonQuantifications = 0D;
            FractionTrueZeros = 1 - CorrectedWeightedAgriculturalUseFraction;
            return !double.IsNaN(MaximumResidueLimit) && MaximumResidueLimit > 0;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (random.NextDouble() < CorrectedWeightedAgriculturalUseFraction) {
                return FractionOfMrl * MaximumResidueLimit;
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
            return FractionOfMrl * MaximumResidueLimit;
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
        /// <param name="random"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the distribution mean
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return CorrectedWeightedAgriculturalUseFraction * FractionOfMrl * MaximumResidueLimit;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
