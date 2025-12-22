using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    /// <summary>
    /// A concentration model based on a degenerate or constant ditsribution, with a single possible value.
    /// </summary>
    public sealed class CMConstant : ConcentrationModel {

        public override ConcentrationModelType ModelType => ConcentrationModelType.Constant;

        /// <summary>
        /// The single point concentration value of the constant distribution.
        /// </summary>
        public double SinglePointConcentration { get; set; }

        /// <summary>
        /// Override: computes the model parameters
        /// </summary>
        public override bool CalculateParameters() {
            FractionPositives = CorrectedOccurenceFraction;
            FractionCensored = 0D;
            FractionNonDetects = 0D;
            FractionNonQuantifications = 0D;
            FractionTrueZeros = 1 - CorrectedOccurenceFraction;
            SinglePointConcentration = ConcentrationDistribution?.Mean ?? 0D;
            return true;
        }

        /// <summary>
        /// Draw from distribution, which is just one discrete point value.
        /// </summary>
        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return SinglePointConcentration;
        }

        /// <summary>
        /// See DrawFromDistribution.
        /// </summary>
        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return DrawFromDistribution(random, nonDetectsHandlingMethod);
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
            return SinglePointConcentration;
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
