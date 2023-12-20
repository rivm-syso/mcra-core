using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsSection : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<HbmSampleConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; } = new();
        public List<IndividualContributionsRecord> IndividualContributionRecords { get; set; } = new();

        public void SummarizeBoxPlots(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances
        ) {
            var ratioSumByIndividual = individualEffects
                .Select(c => (
                    Sum: c.ExposureHazardRatio,
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);

            foreach (var targetCollection in individualEffectsBySubstances) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {
                    var contributions = targetSubstanceIndividualEffects.Value
                        .Select(c => c.ExposureHazardRatio / ratioSumByIndividual[c.SimulatedIndividualId] * 100)
                        .ToList();
                    var samplingWeights = targetSubstanceIndividualEffects.Value
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var (boxPlotRecord, contributionRecord) = summarizeBoxPlot(
                        targetCollection.Target,
                        targetSubstanceIndividualEffects.Key,
                        contributions,
                        samplingWeights
                    );
                    HbmBoxPlotRecords.Add(boxPlotRecord);
                    IndividualContributionRecords.Add(contributionRecord);
                }
            }
        }

        private (HbmSampleConcentrationPercentilesRecord, IndividualContributionsRecord) summarizeBoxPlot(
            ExposureTarget target,
            Compound substance,
            List<double> individualContributions,
            List<double> samplingWeights
        ) {
            var meanContribution = individualContributions.Zip(samplingWeights, (i, w) => i * w).Sum() / samplingWeights.Sum();
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var percentiles = individualContributions
                .PercentilesWithSamplingWeights(samplingWeights, percentages)
                .ToList();
            var positives = individualContributions
                .Where(r => r > 0)
                .ToList();
            var outliers = positives
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c).ToList();
            var boxPlotRecord = new HbmSampleConcentrationPercentilesRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                Description = substance.Name,
                Percentiles = percentiles.ToList(),
                NumberOfPositives = samplingWeights.Count,
                Percentage = samplingWeights.Count * 100d / individualContributions.Count,
                Outliers = outliers.ToList(),
                NumberOfOutLiers = outliers.Count
            };
            var contributionRecord = new IndividualContributionsRecord() {
                TargetUnit = target,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                Contribution = meanContribution,
                Contributions = new List<double>()
            };
            return (boxPlotRecord, contributionRecord);
        }

        public void SummarizeUncertain(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double lowerBound,
            double upperBound

        ) {
            var ratioSumByIndividual = individualEffects
                .Select(c => (
                    Sum: c.ExposureHazardRatio,
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);
            
            foreach (var targetCollection in individualEffectsBySubstances) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {

                    var meanContribution = targetSubstanceIndividualEffects.Value
                        .Sum(c => c.ExposureHazardRatio / ratioSumByIndividual[c.SimulatedIndividualId] * 100 * c.SamplingWeight)
                        / targetSubstanceIndividualEffects.Value.Sum(c => c.SamplingWeight);
                    
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
