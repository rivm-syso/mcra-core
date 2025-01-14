using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsSection : ContributionsForIndividualsSectionBase {
        public void SummarizeBoxPlotsTotalDistribution(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            bool showOutliers
        ) {
            ShowOutliers = showOutliers;
            (IndividualContributionRecords, BoxPlotRecords) = SummarizeBoxPlots(individualEffects, individualEffectsBySubstances);
        }

        public void SummarizeUncertainTotalDistribution(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double lowerBound,
            double upperBound
        ) {
            var ratioSumByIndividual = individualEffects
                .ToDictionary(
                    r => r.SimulatedIndividualId,
                    r => r.ExposureHazardRatio
                );

            foreach (var targetCollection in individualEffectsBySubstances) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {
                    var contributions = targetSubstanceIndividualEffects.Value
                        .Select(c => (
                             Contribution: c.ExposureHazardRatio / ratioSumByIndividual[c.SimulatedIndividualId] * 100,
                             SamplingWeight: c.SimulatedIndividual.SamplingWeight
                           )
                        )
                        .Where(c => !double.IsNaN(c.Contribution))
                        .ToList();
                    var meanContribution = contributions.Sum(c => c.Contribution * c.SamplingWeight) / contributions.Sum(c => c.SamplingWeight);
                    var record = IndividualContributionRecords
                        .Where(c => c.SubstanceCode == targetSubstanceIndividualEffects.Key.Code && c.TargetUnit == targetCollection.Target)
                        .SingleOrDefault();
                    if (record != null) {
                        record.Contributions.Add(meanContribution);
                        record.UncertaintyLowerBound = lowerBound;
                        record.UncertaintyUpperBound = upperBound;
                    }
                }
            }
        }
    }
}
