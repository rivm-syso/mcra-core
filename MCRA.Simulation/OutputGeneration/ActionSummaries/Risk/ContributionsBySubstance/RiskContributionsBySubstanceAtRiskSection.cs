using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceAtRiskSection
        : RiskContributionsBySubstanceUpperSection {

        public void SummarizeUpperAtRisk(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
            double threshold
        ) {
            summarizeUpperAtRisk(
                individualEffects,
                individualEffectsBySubstance,
                lowerPercentage,
                upperPercentage,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isInverseDistribution,
                riskMetricType,
                null,
                threshold
            );
        }

        public void SummarizeUpperAtRiskUncertain(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            RiskMetricType riskMetricType,
            double threshold
        ) {
            var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                ? individualEffects.Where(c => c.HazardExposureRatio < threshold)
                    .Sum(c => c.SamplingWeight)
                : individualEffects.Where(c => c.ExposureHazardRatio > threshold)
                    .Sum(c => c.SamplingWeight);
            var sumAllWeights = individualEffects
                .Sum(c => c.SamplingWeight);
            var percentageForUpperTail = (100 - 100d * sumWeightsCriticalEffect / sumAllWeights);
            //Occasionally very small percentages occur, round them to zero
            if (percentageForUpperTail < 0) {
                percentageForUpperTail = 0;
            }
            summarizeUpperUncertainty(individualEffects, individualEffectsBySubstance, percentageForUpperTail);
        }
    }
}
