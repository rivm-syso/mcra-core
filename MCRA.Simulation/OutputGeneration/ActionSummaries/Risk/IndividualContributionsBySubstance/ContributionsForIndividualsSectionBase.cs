using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance {
    public abstract class ContributionsForIndividualsSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public bool ShowOutliers { get; set; }
        public List<ContributionsForIndividualsPercentileRecord> BoxPlotRecords { get; set; } = [];
        public List<RiskContributionBySubstanceRecord> IndividualContributionRecords { get; set; } = [];

        protected void summarize(
            PopulationStratifier stratifier,
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            int uncertaintyCycles
        ) {
            var ratioSumByIndividual = cumulativeIndividualRisks
                .ToDictionary(
                    r => r.SimulatedIndividualId,
                    r => r.ExposureHazardRatio
                );
            var individualEffects = getIndividualEffects(individualEffectsBySubstance, stratifier);

            //Remove individuals without exposure
            foreach (var targetCollection in individualEffects) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.Effects) {
                    var boxPlotRecord = summarizeBoxPlot(
                        targetCollection.Target,
                        ratioSumByIndividual,
                        targetSubstanceIndividualEffects.Key,
                        targetSubstanceIndividualEffects.Value
                    );
                    var contributionRecord = summarizeContribution(
                        targetCollection.Target,
                        ratioSumByIndividual,
                        targetSubstanceIndividualEffects.Key,
                        targetSubstanceIndividualEffects.Value,
                        uncertaintyCycles,
                        null
                    );
                    if (contributionRecord.Contribution > 0.1) {
                        BoxPlotRecords.Add(boxPlotRecord);
                    }
                    IndividualContributionRecords.Add(contributionRecord);
                }
            }

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                foreach (var targetCollection in individualEffects) {
                    foreach (var targetSubstanceIndividualEffects in targetCollection.Effects) {
                        var results = summarizeContributions(
                            targetCollection.Target,
                            ratioSumByIndividual,
                            targetSubstanceIndividualEffects.Key,
                            targetSubstanceIndividualEffects.Value,
                            uncertaintyCycles
                        );
                        IndividualContributionRecords.AddRange(results);
                    }
                }
            }
            IndividualContributionRecords = [.. IndividualContributionRecords.OrderByDescending(c => c.Contribution)];
        }

        private static ContributionsForIndividualsPercentileRecord summarizeBoxPlot(
            ExposureTarget target,
            Dictionary<int, double> ratioSumByIndividual,
            Compound substance,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> targetSubstanceIndividualEffects
        ) {
            var individualContributions = targetSubstanceIndividualEffects
                .Select(c => (
                     Contribution: c.TotalEndPointExposure / ratioSumByIndividual[c.SimulatedIndividualId] * 100,
                     SamplingWeight: c.SamplingWeight
                   )
                )
                .Where(c => !double.IsNaN(c.Contribution))
                .ToList();

            var samplingWeights = individualContributions.Select(c => c.SamplingWeight).ToList();
            var contributions = individualContributions.Select(c => c.Contribution).ToList();
            var meanContribution = individualContributions.Sum(c => c.Contribution * c.SamplingWeight) / samplingWeights.Sum();
            var result = new List<HbmSubstancePercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var percentiles = contributions
                .PercentilesWithSamplingWeights(samplingWeights, percentages)
                .ToList();
            var positives = contributions
                .Where(r => r > 0)
                .ToList();
            var outliers = positives
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .ToList();
            var boxPlotRecord = new ContributionsForIndividualsPercentileRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                Percentiles = [.. percentiles],
                NumberOfPositives = samplingWeights.Count,
                Percentage = samplingWeights.Count * 100d / contributions.Count,
                Outliers = [.. outliers],
                NumberOfOutLiers = outliers.Count
            };
            return boxPlotRecord;
        }

        private static RiskContributionBySubstanceRecord summarizeContribution(
            ExposureTarget target,
            Dictionary<int, double> ratioSumByIndividual,
            Compound substance,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> targetSubstanceIndividualEffects,
            int uncertaintyCycles,
            IStratificationLevel stratificationLevel
        ) {
            var individualContributions = targetSubstanceIndividualEffects
                .Select(c => (
                     Contribution: c.TotalEndPointExposure / ratioSumByIndividual[c.SimulatedIndividualId] * 100,
                     SamplingWeight: c.SamplingWeight
                   )
                )
                .Where(c => !double.IsNaN(c.Contribution))
                .ToList();

            var samplingWeights = individualContributions.Select(c => c.SamplingWeight).ToList();
            var meanContribution = individualContributions.Sum(c => c.Contribution * c.SamplingWeight) / samplingWeights.Sum();
            var descriptor = new SubstanceContributorKey() { Substance = substance };
            var stratificationLevelParts = (stratificationLevel as CompositeStratificationLevel)?.Parts;
            var contributionRecord = new RiskContributionBySubstanceRecord() {
                Stratification = stratificationLevel?.Name,
                StratificationLevelCode = stratificationLevel?.Code,
                StratificationSubLevelCodes = stratificationLevelParts?.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code],
                StratificationSubLevelNames = stratificationLevelParts?.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name],
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                Contribution = meanContribution,
                Contributions = [],
                UncertaintyCycles = uncertaintyCycles
            };
            contributionRecord.SetDescriptorValues(descriptor);
            return contributionRecord;
        }

        private static List<RiskContributionBySubstanceRecord> summarizeContributions(
            ExposureTarget target,
            Dictionary<int, double> ratioSumByIndividual,
            Compound substance,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> targetSubstanceIndividualEffects,
            int uncertaintyCycles
        ) {
            var groups = targetSubstanceIndividualEffects
                .GroupBy(c => c.StratificationLevel)
                .ToList();
            var results = new List<RiskContributionBySubstanceRecord>();
            foreach (var group in groups) {
                var stratifiedContributionRecord = summarizeContribution(
                    target,
                    ratioSumByIndividual,
                    substance,
                    targetSubstanceIndividualEffects,
                    uncertaintyCycles,
                    group.Key
                );
                results.Add(stratifiedContributionRecord);
            }
            return results;
        }

        protected void updateRecords(
            PopulationStratifier stratifier,
            Dictionary<int, double> ratioSumByIndividual,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double lowerBound,
            double upperBound
        ) {
            var individualEffects = getIndividualEffects(individualEffectsBySubstances, stratifier);

            //Remove individuals without exposure
            foreach (var targetCollection in individualEffects) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.Effects) {
                    update(
                        ratioSumByIndividual,
                        targetCollection.Target,
                        targetSubstanceIndividualEffects.Key,
                        targetSubstanceIndividualEffects.Value,
                        null,
                        lowerBound,
                        upperBound
                    );
                }
            }
        }

        private void update(
            Dictionary<int, double> ratioSumByIndividual,
            ExposureTarget target,
            Compound substance,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> targetSubstanceIndividualEffects,
            IStratificationLevel stratificationLevel,
            double lowerBound,
            double upperBound
        ) {
            var contributions = targetSubstanceIndividualEffects
                .Select(c => (
                     Contribution: c.TotalEndPointExposure / ratioSumByIndividual[c.SimulatedIndividualId] * 100,
                     SamplingWeight: c.SamplingWeight
                   )
                )
                .Where(c => !double.IsNaN(c.Contribution))
                .ToList();
            var meanContribution = contributions.Sum(c => c.Contribution * c.SamplingWeight) / contributions.Sum(c => c.SamplingWeight);
            var record = IndividualContributionRecords
                .SingleOrDefault(c => c.SubstanceCode == substance.Code && c.BiologicalMatrix == target.GetDisplayName()
                    && c.StratificationLevelCode == stratificationLevel?.Code);
            if (record != null) {
                record.Contributions.Add(meanContribution);
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
            }
        }

        protected void updateStratifiedRecords(
            PopulationStratifier stratifier,
            Dictionary<int, double> ratioSumByIndividual,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double lowerBound,
            double upperBound
        ) {
            var individualEffects = getIndividualEffects(individualEffectsBySubstances, stratifier);
            //Remove individuals without exposure
            foreach (var targetCollection in individualEffects) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.Effects) {
                    var groups = targetSubstanceIndividualEffects.Value
                        .GroupBy(c => c.StratificationLevel)
                        .ToList();
                    var results = new List<RiskContributionBySubstanceRecord>();
                    foreach (var group in groups) {
                        update(
                            ratioSumByIndividual,
                            targetCollection.Target,
                            targetSubstanceIndividualEffects.Key,
                            [.. group],
                            group.Key,
                            lowerBound,
                            upperBound
                        );
                    }
                }
            }
        }

        protected List<(ExposureTarget Target, Dictionary<Compound, List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>>> Effects)> getIndividualEffects(
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            PopulationStratifier stratifier
        ) {
            var result = individualEffectsBySubstance
                .Select(r => {
                    var target = r.Target;
                    var results = r.SubstanceIndividualEffects
                        .Select(c => (substance: c.Key, effects: c.Value.Select((i, ix) => new RiskIndividualEffectsByDescriptor<SubstanceContributorKey> {
                            SamplingWeight = i.SimulatedIndividual.SamplingWeight,
                            SimulatedIndividualId = i.SimulatedIndividualId,
                            StratificationLevel = stratifier?.GetLevel(i.SimulatedIndividual),
                            TotalExposure = r.SubstanceIndividualEffects.Sum(q => q.Value[ix].ExposureHazardRatio * q.Value[ix].SimulatedIndividual.SamplingWeight),
                            TotalEndPointExposure = i.ExposureHazardRatio * i.SimulatedIndividual.SamplingWeight,
                        }).ToList()));
                    return (target, effects: results.ToDictionary(c => c.substance, c => c.effects));
                }).ToList();
            return result;
        }
    }
}