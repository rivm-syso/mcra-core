using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskContributionsBySubstanceUpperSection : RiskContributionsBySubstanceSectionBase {

        public void SummarizeUpper(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
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
                null
            );
        }

        public void SummarizeUpperUncertain(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double percentageForUpperTail
        ) {
            summarizeUpperUncertainty(
                cumulativeIndividualRisks,
                individualEffectsBySubstance,
                percentageForUpperTail
            );
        }

        /// <summary>
        /// Summarize risk substances distribution.
        /// </summary>
        protected void summarizeUpperAtRisk(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
            double? percentageForUpperTail,
            double? threshold
        ) {
            if (threshold.HasValue) {
                var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                    ? cumulativeIndividualRisks.Where(c => c.HazardExposureRatio < threshold.Value)
                        .Sum(c => c.SimulatedIndividual.SamplingWeight)
                    : cumulativeIndividualRisks.Where(c => c.ExposureHazardRatio > threshold.Value)
                        .Sum(c => c.SimulatedIndividual.SamplingWeight);
                var sumAllWeights = cumulativeIndividualRisks
                    .Sum(c => c.SimulatedIndividual.SamplingWeight);
                percentageForUpperTail = 100 - 100d * sumWeightsCriticalEffect / sumAllWeights;
            }
            _riskPercentages = [50];
            _isInverseDistribution = isInverseDistribution;
            var weights = cumulativeIndividualRisks.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var percentile = cumulativeIndividualRisks.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail.Value);
            var individualEffectsUpper = cumulativeIndividualRisks
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();
            PercentagesAtRisk = new PercentageAtRiskRecord {
                Percentage = individualEffectsUpper.Sum(c => c.SimulatedIndividual.SamplingWeight) / weights.Sum() * 100,
                Percentages = []
            };
            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffectsUpper);

            var individualEffects = getIndividualEffects(individualEffectsBySubstance, null);
            ContributionRecords = individualEffects.SelectMany(c =>
                c.Effects,
                    (c, dict) => (c.Target, dict.Key, dict.Value)
                )
                .AsParallel()
                .Select(kvp => createContributionRecord(
                    individualEffects.First().Target,
                    [.. kvp.Value.Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId))],
                    kvp.Key,
                    riskMetricType,
                    null
                ))
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.SubstanceCode)
                .ToList();
        }

        protected void summarizeUpperUncertainty(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double percentageForUpperTail
        ) {
            var weights = cumulativeIndividualRisks.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var percentile = cumulativeIndividualRisks.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = cumulativeIndividualRisks
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            PercentagesAtRisk.Percentages.Add(individualEffectsUpper.Sum(c => c.SimulatedIndividual.SamplingWeight) / weights.Sum() * 100);
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();

            var individualEffects = getIndividualEffects(individualEffectsBySubstance, null);
            var upperIndividualEffects = individualEffects.SelectMany(c =>
                c.Effects,
                    (c, dict) => (c.Target, dict.Key, dict.Value)
                )
                .AsParallel()
                .Select(c => (
                    target: c.Target,
                    substance: c.Key,
                    effects: c.Value.Where(r => simulatedIndividualIds.Contains(r.SimulatedIndividualId)).ToList()
                ))
                .GroupBy(c => c.target)
                .Select(c => (
                    target: c.Key,
                    substanceEffects: c.ToDictionary(d => d.substance, d => d.effects)
                ))
                .ToList();

            updateRecords(
                individualEffects: upperIndividualEffects
            );
        }
    }
}
