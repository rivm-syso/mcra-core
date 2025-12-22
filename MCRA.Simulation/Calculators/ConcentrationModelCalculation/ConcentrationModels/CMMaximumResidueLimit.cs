using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    public sealed class CMMaximumResidueLimit : ConcentrationModel {
        public override ConcentrationModelType ModelType => ConcentrationModelType.MaximumResidueLimit;

        /// <summary>
        /// Override: computes the model parameters
        /// </summary>
        public override bool CalculateParameters() {
            FractionPositives = CorrectedOccurenceFraction;
            FractionCensored = 0D;
            FractionNonDetects = 0D;
            FractionNonQuantifications = 0D;
            FractionTrueZeros = 1 - CorrectedOccurenceFraction;
            return !double.IsNaN(MaximumResidueLimit) && MaximumResidueLimit > 0;
        }

        /// <summary>
        /// Draw from full distribution (zero, censored or positive)
        /// </summary>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (random.NextDouble() < CorrectedOccurenceFraction) {
                return FractionOfMrl * MaximumResidueLimit;
            }
            return 0D;
        }

        /// <summary>
        /// Draw from censored distribution (censored or positive)
        /// </summary>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return FractionOfMrl * MaximumResidueLimit;
        }

        /// <summary>
        /// Replace nondetects according to the NonDetectsHandlingMethod
        /// </summary>
        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the distribution mean
        /// </summary>
        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return CorrectedOccurenceFraction * FractionOfMrl * MaximumResidueLimit;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
