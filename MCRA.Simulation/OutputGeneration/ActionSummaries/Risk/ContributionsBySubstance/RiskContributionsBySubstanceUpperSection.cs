using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskContributionsBySubstanceUpperSection : RiskContributionsBySubstanceSectionBase {

        public void SummarizeUpper(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
            double percentageForUpperTail
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
                percentageForUpperTail,
                null
            );
        }

        public void SummarizeUpperUncertain(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            RiskMetricType riskMetricType,
            double percentageForUpperTail
        ) {
            summarizeUpperUncertainty(individualEffects, individualEffectsBySubstance, percentageForUpperTail);
        }

        /// <summary>
        /// Summarize risk substances distribution.
        /// </summary>
        protected void summarizeUpperAtRisk(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            RiskMetricType riskMetricType,
            double? percentageForUpperTail,
            double? threshold
        ) {
            if (threshold.HasValue) {
                var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                    ? individualEffects.Where(c => c.HazardExposureRatio < threshold.Value)
                        .Sum(c => c.SamplingWeight)
                    : individualEffects.Where(c => c.ExposureHazardRatio > threshold.Value)
                        .Sum(c => c.SamplingWeight);
                var sumAllWeights = individualEffects
                    .Sum(c => c.SamplingWeight);
                percentageForUpperTail = 100 - 100d * sumWeightsCriticalEffect / sumAllWeights;
            }
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _riskPercentages = [_lowerPercentage, 50, _upperPercentage];
            _isInverseDistribution = isInverseDistribution;
            UpperPercentage = 100 - percentageForUpperTail.Value;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail.Value);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();
            CalculatedUpperPercentage = individualEffectsUpper.Sum(c => c.SamplingWeight) / weights.Sum() * 100;
            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffectsUpper);

            Records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(kvp => createSubstanceSummaryRecord(
                    kvp.Value.Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId)).ToList(),
                    kvp.Key,
                    totalExposure,
                    riskMetricType
                ))
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.SubstanceCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        protected void summarizeUpperUncertainty(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double percentageForUpperTail
        ) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();

            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffectsUpper);
            var records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(r => new RiskBySubstanceRecord() {
                    SubstanceCode = r.Key.Code,
                    Contribution = CalculateExposureHazardWeightedTotal(r.Value.Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId)).ToList()) / totalExposure
                })
                .ToList();
            if (simulatedIndividualIds.Any()) {
                updateContributions(records);
            }
        }
    }
}
