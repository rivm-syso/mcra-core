using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleRiskRatioSection : RiskCharacterisationRatioSectionBase<RiskSubstanceContributorKey, SubstanceRiskDistributionRecord> {

        public List<TargetUnit> TargetUnits { get; set; }

        /// <summary>
        /// Multiple risk records (per traget).
        /// </summary>
        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> MultipleRiskRecords { get; set; } = [];

        /// <summary>
        /// Multiple risk records (per traget).
        /// </summary>
        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> StratifiedMultipleRiskRecords { get; set; } = [];

        public void Summarize(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<TargetUnit> targetUnits,
            ICollection<Compound> substances,
            Effect effect,
            double confidenceInterval,
            double threshold,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative,
            PopulationStratifier stratifier,
            bool skipPrivacySensitiveOutputs
        ) {
            var pLower = (100 - confidenceInterval) / 2;
            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                    pUpper = maxUpperPercentile;
                    pLower = 100 - pUpper;
                    confidenceInterval = 100 - 2 * pLower;
                }
            }
            TargetUnits = targetUnits;
            StratificationVariables = stratifier?.GetStratificationVariables();
            EffectName = effect?.Name;
            RiskMetric = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            ReferenceDose = referenceDose?.Value ?? double.NaN;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            RiskBarPercentages = [pLower, 50, pUpper];

            var cumulativeTarget = targetUnits.Count == 1 ? targetUnits.First().Target : null;
            var orderedTargetUnits = targetUnits.OrderByDescending(r => r?.Target == cumulativeTarget).ToList();
            foreach (var targetUnit in orderedTargetUnits) {
                var target = targetUnit?.Target;
                var targetSummaryRecords = createMultipleRisksRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections?
                        .SingleOrDefault(c => c.Target == target).IndividualEffects,
                    individualEffects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative && target == cumulativeTarget,
                    stratifier,
                    false
                );
                MultipleRiskRecords.Add((target, targetSummaryRecords));
            }

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                foreach (var targetUnit in orderedTargetUnits) {
                    var target = targetUnit?.Target;
                    var targetSummaryRecords = createMultipleRisksRecords(
                        target,
                        substances,
                        individualEffectsBySubstanceCollections?
                            .SingleOrDefault(c => c.Target == target).IndividualEffects,
                        individualEffects,
                        riskMetricCalculationType,
                        isInverseDistribution,
                        isCumulative && target == cumulativeTarget,
                        stratifier,
                        true
                    );
                    StratifiedMultipleRiskRecords.Add((target, targetSummaryRecords));
                }
            }
        }

        /// <summary>
        /// Summarizes uncertainty for risks safety charts single substance.
        /// </summary>
        public void SummarizeUncertain(
            ICollection<TargetUnit> targetUnits,
            ICollection<Compound> substances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative,
            PopulationStratifier stratifier
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;

            var cumulativeTarget = targetUnits.Count == 1 ? targetUnits.First().Target : null;
            foreach (var targetUnit in targetUnits) {
                var target = targetUnit?.Target;
                var recordsLookup = MultipleRiskRecords.SingleOrDefault(c => c.Target == target).Records
                    .ToDictionary(r => (r.GetDescriptorKey(), r.StratificationLevelCode), r => r);
                var records = createMultipleRisksRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections.SingleOrDefault(c => c.Target == target).IndividualEffects,
                    individualEffects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative && target == cumulativeTarget,
                    stratifier,
                    false
                );
                foreach (var item in records) {
                    var record = recordsLookup[(item.GetDescriptorKey(), item.StratificationLevelCode)];
                    record.UncertaintyLowerLimit = uncertaintyLowerBound;
                    record.UncertaintyUpperLimit = uncertaintyUpperBound;
                    record.RiskPercentiles.AddUncertaintyValues([item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom]);
                    record.ProbabilityOfCriticalEffects.AddUncertaintyValues([item.ProbabilityOfCriticalEffect]);
                }
            }

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                foreach (var targetUnit in targetUnits) {
                    var target = targetUnit?.Target;
                    var recordsLookup = StratifiedMultipleRiskRecords.SingleOrDefault(c => c.Target == target).Records
                        .ToDictionary(r => (r.GetDescriptorKey(), r.StratificationLevelCode), r => r);
                    var records = createMultipleRisksRecords(
                        target,
                        substances,
                        individualEffectsBySubstanceCollections?
                            .SingleOrDefault(c => c.Target == target).IndividualEffects,
                        individualEffects,
                        riskMetricCalculationType,
                        isInverseDistribution,
                        isCumulative && target == cumulativeTarget,
                        stratifier,
                        true
                    );
                    foreach (var item in records) {
                        var record = recordsLookup[(item.GetDescriptorKey(), item.StratificationLevelCode)];
                        record.UncertaintyLowerLimit = uncertaintyLowerBound;
                        record.UncertaintyUpperLimit = uncertaintyUpperBound;
                        record.RiskPercentiles.AddUncertaintyValues([item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom]);
                        record.ProbabilityOfCriticalEffects.AddUncertaintyValues([item.ProbabilityOfCriticalEffect]);
                    }
                }
            }
        }

        private List<SubstanceRiskDistributionRecord> createMultipleRisksRecords(
           ExposureTarget target,
           ICollection<Compound> substances,
           Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
           List<IndividualEffect> individualEffects,
           RiskMetricCalculationType riskMetricCalculationType,
           bool isInverseDistribution,
           bool isCumulative,
           PopulationStratifier stratifier,
           bool stratify
        ) {
            var results = new List<SubstanceRiskDistributionRecord>();

            if (substances.Count > 1 && individualEffects != null && isCumulative) {
                var riskReference = getRiskReference(riskMetricCalculationType);
                var descriptor = new RiskSubstanceContributorKey() {
                    Substance = riskReference
                };
                var records = getRiskRecords(
                    target,
                    individualEffects,
                    isInverseDistribution,
                    isCumulative,
                    stratifier,
                    descriptor,
                    stratify
                );
                results.AddRange(records);
            }

            if (individualEffectsBySubstance?.Count > 0) {
                foreach (var substance in substances) {
                    var descriptor = new RiskSubstanceContributorKey() {
                        Substance = substance
                    };
                    if (individualEffectsBySubstance.TryGetValue(substance, out var result)) {
                        var records = getRiskRecords(
                            target,
                            result,
                            isInverseDistribution,
                            isCumulative,
                            stratifier,
                            descriptor,
                            stratify
                        );
                        results.AddRange(records);
                    }
                }
            }

            return results.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }

        private List<SubstanceRiskDistributionRecord> getRiskRecords(
            ExposureTarget target,
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            bool isCumulative,
            PopulationStratifier stratifier,
            RiskSubstanceContributorKey descriptor,
            bool stratify
        ) {
            var results = new List<SubstanceRiskDistributionRecord>();
            var effects = getIndividualEffects(individualEffects, stratifier);
            var groups = effects
                .GroupBy(c => stratify ? c.StratificationLevel : null);
            foreach (var group in groups) {
                var record = createSubstanceRiskRecord(
                    target,
                    [.. group],
                    isCumulative,
                    isInverseDistribution,
                    descriptor,
                    group.Key
                );
                results.Add(record);
            }
            return results;
        }
    }
}
