using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceAtRiskSection : RiskContributionsBySubstanceUpperSection {

        public void SummarizeUpperAtRisk(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
            double threshold,
            double percentageForUpperTail,
            int uncertaintyCycles
        ) {
            _lowerBound = uncertaintyLowerBound;
            _upperBound = uncertaintyUpperBound;
            _uncertaintyCycles = uncertaintyCycles;
            summarizeUpperAtRisk(
                cumulativeIndividualRisks,
                individualEffectsBySubstance,
                isInverseDistribution,
                riskMetricType,
                percentageForUpperTail,
                threshold
            );
        }

        public void SummarizeUpperAtRiskUncertain(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            RiskMetricType riskMetricType,
            double threshold
        ) {
            var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                ? cumulativeIndividualRisks.Where(c => c.HazardExposureRatio < threshold)
                    .Sum(c => c.SimulatedIndividual.SamplingWeight)
                : cumulativeIndividualRisks.Where(c => c.ExposureHazardRatio > threshold)
                    .Sum(c => c.SimulatedIndividual.SamplingWeight);
            var sumAllWeights = cumulativeIndividualRisks
                .Sum(c => c.SimulatedIndividual.SamplingWeight);
            var percentageForUpperTail = (100 - 100d * sumWeightsCriticalEffect / sumAllWeights);
            //Occasionally very small percentages occur, round them to zero
            if (percentageForUpperTail < 0) {
                percentageForUpperTail = 0;
            }
            summarizeUpperUncertainty(cumulativeIndividualRisks, individualEffectsBySubstance, percentageForUpperTail);
        }
    }
}
