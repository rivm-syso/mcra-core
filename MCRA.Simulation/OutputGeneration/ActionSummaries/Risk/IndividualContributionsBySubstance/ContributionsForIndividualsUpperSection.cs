using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ContributionsForIndividualsUpperSection : ContributionsForIndividualsSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public bool IsPercentageAtRisk { get; set; }

        public virtual void Summarize(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double percentageForUpperTail,
            bool showOutliers
        ) {
            summarizeUpperIndividualContributions(
                individualEffects,
                individualEffectsBySubstances,
                riskMetricType,
                percentageForUpperTail,
                null,
                showOutliers
            );
        }

        public virtual void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double percentageForUpperTail,
            double uncertaintyLower,
            double uncertaintyUpper
        ) {
            summarizeUncertainUpperDistribution(
                individualEffects,
                individualEffectsBySubstances,
                riskMetricType,
                percentageForUpperTail,
                null,
                uncertaintyLower,
                uncertaintyUpper
            );
        }
        protected void summarizeUpperIndividualContributions(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double? percentageForUpperTail,
            double? threshold,
            bool showOutliers
        ) {
            if (threshold.HasValue) {
                IsPercentageAtRisk = true;
                var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                    ? individualEffects.Where(c => c.HazardExposureRatio < threshold.Value)
                        .Sum(c => c.SamplingWeight)
                    : individualEffects.Where(c => c.ExposureHazardRatio > threshold.Value)
                        .Sum(c => c.SamplingWeight);
                var sumAllWeights = individualEffects
                    .Sum(c => c.SamplingWeight);
                percentageForUpperTail = 100 - 100d * sumWeightsCriticalEffect / sumAllWeights;
            }

            ShowOutliers = showOutliers;
            UpperPercentage = 100 - percentageForUpperTail.Value;

            //Select the individuals in the upper tail
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail.Value);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();

            var individualEffectsBySubstancesUpper = new List<(ExposureTarget Target,
                Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)>();
            foreach (var targetCollection in individualEffectsBySubstances) {
                var collection = new Dictionary<Compound, List<IndividualEffect>>();
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {
                    var result = targetSubstanceIndividualEffects.Value
                         .Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId))
                         .ToList();
                    collection[targetSubstanceIndividualEffects.Key] = result;
                }
                individualEffectsBySubstancesUpper.Add((targetCollection.Target, collection));
            }
            CalculatedUpperPercentage = individualEffectsUpper.Sum(c => c.SamplingWeight) / weights.Sum() * 100;
            (IndividualContributionRecords, HbmBoxPlotRecords) = SummarizeBoxPlots(individualEffectsUpper, individualEffectsBySubstancesUpper);
        }

        protected void summarizeUncertainUpperDistribution(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double? percentageForUpperTail,
            double? threshold,
            double lowerBound,
            double upperBound
        ) {
            if (threshold.HasValue) {
                IsPercentageAtRisk = true;
                var sumWeightsCriticalEffect = riskMetricType == RiskMetricType.HazardExposureRatio
                    ? individualEffects.Where(c => c.HazardExposureRatio < threshold.Value)
                        .Sum(c => c.SamplingWeight)
                    : individualEffects.Where(c => c.ExposureHazardRatio > threshold.Value)
                        .Sum(c => c.SamplingWeight);
                var sumAllWeights = individualEffects
                    .Sum(c => c.SamplingWeight);
                percentageForUpperTail = 100 - 100d * sumWeightsCriticalEffect / sumAllWeights;
            }
            //Select the individuals in the upper tail
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail.Value);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();

            var ratioSumByIndividual = individualEffectsUpper
                .Select(c => (
                    Sum: c.ExposureHazardRatio,
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);

            foreach (var targetCollection in individualEffectsBySubstances) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {
                    var contributions = targetSubstanceIndividualEffects.Value
                        .Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId))
                        .Select(c => (
                             Contribution: c.ExposureHazardRatio / ratioSumByIndividual[c.SimulatedIndividualId] * 100,
                             SamplingWeight: c.SamplingWeight
                           )
                        )
                        .Where(c => !double.IsNaN(c.Contribution))
                        .ToList();
                    var meanContribution = contributions.Sum(c => c.Contribution * c.SamplingWeight) / contributions.Sum(c => c.SamplingWeight);
                    var record = IndividualContributionRecords
                        .Where(c => c.SubstanceCode == targetSubstanceIndividualEffects.Key.Code && c.TargetUnit == targetCollection.Target)
                        .SingleOrDefault();
                    if (record != null && simulatedIndividualIds.Any()) {
                        record.Contributions.Add(meanContribution);
                        record.UncertaintyLowerBound = lowerBound;
                        record.UncertaintyUpperBound = upperBound;
                    }
                }
            }
        }
    }
}
