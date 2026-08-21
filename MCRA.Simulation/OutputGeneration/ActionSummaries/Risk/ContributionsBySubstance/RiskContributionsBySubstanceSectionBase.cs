using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskContributionsBySubstanceSectionBase : AtRiskSectionBase {

        public override bool SaveTemporaryData => true;

        protected double _lowerBound;
        protected double _upperBound;
        protected int _uncertaintyCycles;
        public PercentageAtRiskRecord PercentagesAtRisk = new();
        public List<RiskContributionBySubstanceRecord> ContributionRecords { get; set; }

        protected RiskContributionBySubstanceRecord createContributionRecord(
            ExposureTarget target,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> individualEffects,
            Compound substance,
            RiskMetricType riskMetricType,
            IStratificationLevel stratificationLevel
        ) {
            var descriptor = new SubstanceContributorKey() { Substance = substance };
            var stratificationLevelParts = (stratificationLevel as CompositeStratificationLevel)?.Parts;
            var total = individualEffects.Sum(c => c.TotalEndPointExposure);
            var sumSamplingWeights = individualEffects.Sum(c => c.SamplingWeight);
            var allWeights = individualEffects
                .Select(c => c.SamplingWeight)
                .ToList();
            var weights = individualEffects.Where(c => c.TotalEndPointExposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var meanAll = weights.Any() ? total / sumSamplingWeights : 0;
            var record = new RiskContributionBySubstanceRecord() {
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                Stratification = stratificationLevel?.Name,
                StratificationLevelCode = stratificationLevel?.Code,
                StratificationSubLevelCodes = stratificationLevelParts?.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code],
                StratificationSubLevelNames = stratificationLevelParts?.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name],
                Contributions = [],
                MeanAll = riskMetricType == RiskMetricType.ExposureHazardRatio ? meanAll : 1 / meanAll,
                Contribution = total / individualEffects.Sum(c => c.TotalExposure) * 100,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
                UncertaintyLowerBound = _lowerBound,
                UncertaintyUpperBound = _upperBound,
                UncertaintyCycles = _uncertaintyCycles
            };
            record.SetDescriptorValues(descriptor);
            return record;
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

        protected List<RiskContributionBySubstanceRecord> createContributionRecords(
            ExposureTarget target,
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> individualEffects,
            Compound substance,
            RiskMetricType riskMetricType,
            bool stratify
        ) {
            var results = new List<RiskContributionBySubstanceRecord>();
            var groups = individualEffects
                .GroupBy(c => stratify ? c.StratificationLevel : null)
                .ToList();

            foreach (var group in groups) {
                var record = createContributionRecord(
                    target,
                    [.. group],
                    substance,
                    riskMetricType,
                    group.Key
                );
                results.Add(record);
            }
            return results;
        }

        protected void updateRecords(List<(ExposureTarget Target, Dictionary<Compound, List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>>> Effects)> individualEffects) {
            _ = individualEffects.SelectMany(c =>
                    c.Effects,
                        (c, dict) => (c.Target, dict.Key, dict.Value)
                    )
                .AsParallel()
                .Select(r => getRecord(
                    [.. r.Value],
                    r.Key,
                    null
                ))
                .ToList();
        }

        protected void updateStratifiedRecords(List<(ExposureTarget Target, Dictionary<Compound, List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>>> Effects)> individualEffects) {
            _ = individualEffects.SelectMany(c =>
                    c.Effects,
                        (c, dict) => (c.Target, dict.Key, dict.Value)
                    )
                .AsParallel()
                .SelectMany(r => getRecords(
                    [.. r.Value],
                    r.Key,
                    true
                ))
                .ToList();
        }

        protected RiskContributionBySubstanceRecord getRecord(
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> individualEffects,
            Compound substance,
            IStratificationLevel stratificationLevel
        ) {
            var descriptor = new SubstanceContributorKey() { Substance = substance };
            var record = new RiskContributionBySubstanceRecord() {
                Contribution = individualEffects.Sum(c => c.TotalEndPointExposure) / individualEffects.Sum(c => c.TotalExposure) * 100,
                StratificationLevelCode = stratificationLevel?.Code,
            };
            record.SetDescriptorValues(descriptor);

            var match = ContributionRecords
                .SingleOrDefault(c => c.GetDescriptorKey() == record.GetDescriptorKey()
                    && c.StratificationLevelCode == stratificationLevel?.Code);
            if (match != null) {
                match.Contributions.Add(record.Contribution);
            } else {
                record.Contributions = [];
                record.Stratification = stratificationLevel?.Name;
                record.StratificationLevelCode = stratificationLevel?.Code;
                record.StratificationSubLevelCodes = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code];
                record.StratificationSubLevelNames = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name];
                record.Contribution = record.Contribution;
                record.UncertaintyLowerBound = _lowerBound;
                record.UncertaintyUpperBound = _upperBound;
                record.UncertaintyCycles = _uncertaintyCycles;
                record.SetDescriptorValues(descriptor);
                ContributionRecords.Add(record);
            }
            return record;
        }

        protected List<RiskContributionBySubstanceRecord> getRecords(
            List<RiskIndividualEffectsByDescriptor<SubstanceContributorKey>> individualEffects,
            Compound substance,
            bool stratify
        ) {
            var results = new List<RiskContributionBySubstanceRecord>();
            var groups = individualEffects
                .GroupBy(c => stratify ? c.StratificationLevel : null)
                .ToList();

            foreach (var group in groups) {
                var record = getRecord(
                    [.. group],
                    substance,
                    group.Key
                );
                results.Add(record);
            }
            return results;
        }
    }
}
