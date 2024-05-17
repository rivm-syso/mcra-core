using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsUpperSection : ContributionsForIndividualsSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        public void SummarizeBoxPlotsUpperDistribution(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double percentageForUpperTail,
            bool showOutliers
        ) {
            ShowOutliers = showOutliers;
            UpperPercentage = 100 - percentageForUpperTail;

            //Select the individuals in the upper tail
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToList();

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

        public void SummarizeUncertainUpperDistribution(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double percentageForUpperTail,
            double lowerBound,
            double upperBound
        ) {
            //Select the individuals in the upper tail
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToList();

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
