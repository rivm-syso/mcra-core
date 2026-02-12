using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Lognormal model.
    /// </summary>
    public sealed class CMLogNormal : ConcentrationModel {

        private LogNormalDistribution _logNormalDistribution;

        public override ConcentrationModelType ModelType => ConcentrationModelType.LogNormal;

        public override bool CalculateParameters() {
            if (ConcentrationDistribution != null && ConcentrationDistribution.CV != null) {
                FractionPositives = CorrectedOccurenceFraction;
                FractionCensored = FractionNonDetects = FractionNonQuantifications = 0D;
                FractionTrueZeros = 1 - CorrectedOccurenceFraction;
                var alignmentFactor = ConcentrationDistribution.ConcentrationUnit.GetConcentrationUnitMultiplier(ConcentrationUnit);
                _logNormalDistribution = LogNormalDistribution.FromMeanAndCv(ConcentrationDistribution.Mean * alignmentFactor, (double)ConcentrationDistribution.CV);
            } else {
                return false;
            }
            return true;
        }

        public override double DrawFromDistribution(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            if (random.NextDouble() < CorrectedOccurenceFraction) {
                return _logNormalDistribution.Draw(random);
            }
            return 0D;
        }

        public override double DrawFromDistributionExceptZeroes(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            return _logNormalDistribution.Draw(random);
        }

        public override double DrawAccordingToNonDetectsHandlingMethod(IRandom random, NonDetectsHandlingMethod nonDetectsHandlingMethod, double fraction) {
            throw new NotImplementedException();
        }

        public override double GetDistributionMean(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            throw new NotImplementedException();
        }

        public override double GetImputedCensoredValue(SampleCompound sampleSubstance, IRandom random) {
            throw new NotImplementedException();
        }
    }
}
