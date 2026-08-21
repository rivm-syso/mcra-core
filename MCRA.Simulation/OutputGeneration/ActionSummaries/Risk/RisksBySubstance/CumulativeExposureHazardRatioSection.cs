using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureHazardRatioSection : ExposureHazardRatioSectionBase<SubstanceContributorKey, SubstanceRiskDistributionRecord> {
        public double UpperBoundConficenceInterval { get; set; }

        public List<TargetUnit> TargetUnits { get; set; }

        public void Summarize(
            List<TargetUnit> targetUnits,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> cumulativeIndividualRisks,
            ICollection<Compound> substances,
            Effect focalEffect,
            RiskMetricCalculationType riskMetricCalculationType,
            RiskMetricType riskMetricType,
            double confidenceInterval,
            double threshold,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool skipPrivacySensitiveOutputs,
            bool isCumulative
        ) {
            TargetUnits = targetUnits;
            EffectName = focalEffect?.Name;
            RiskMetric = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            RiskBarPercentages = [pLower, 50, 100 - pLower];
            UpperBoundConficenceInterval = RiskBarPercentages[2];

            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(cumulativeIndividualRisks.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            var selectedTargets = targetUnits.Where(c => c != null).ToList();
            foreach (var target in selectedTargets) {
                var records = new List<SubstanceRiskDistributionRecord>();
                if (substances.Count > 1 && cumulativeIndividualRisks != null && isCumulative) {
                    var descriptor = new SubstanceContributorKey() {
                        Substance = getRiskReference(RiskMetricCalculationType)
                    };
                    var effects = getIndividualEffects(cumulativeIndividualRisks, null);
                    var record = createSubstanceRiskRecord(target?.Target, effects, true, isInverseDistribution, descriptor, null);
                    records.Add(record);
                }
                var individualEffectsDict = individualEffectsBySubstanceCollections?
                    .SingleOrDefault(c => c.Target == target?.Target).IndividualEffects;
                if (individualEffectsDict?.Count > 0) {
                    foreach (var substance in substances) {
                        var descriptor = new SubstanceContributorKey() {
                            Substance = substance
                        };
                        if (individualEffectsDict.TryGetValue(substance, out var results)) {
                            var effects = getIndividualEffects(results, null);
                            var record = createSubstanceRiskRecord(target?.Target, effects, false, isInverseDistribution, descriptor, null);
                            records.Add(record);
                        }
                    }
                }
                RiskRecords.Add((target?.Target, records));
            }
        }

        public void SummarizeUncertain(
            ICollection<ExposureTarget> targets,
            ICollection<Compound> substances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> collections,
            List<IndividualEffect> cumulativeIndividualRisks,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            foreach (var target in targets) {
                var records = new List<SubstanceRiskDistributionRecord>();
                if (substances.Count > 1 && cumulativeIndividualRisks != null && isCumulative) {
                    var descriptor = new SubstanceContributorKey() {
                        Substance = getRiskReference(RiskMetricCalculationType)
                    };
                    var effects = getIndividualEffects(cumulativeIndividualRisks, null);
                    var record = createSubstanceRiskRecord(target, effects, true, isInverseDistribution, descriptor, null);
                    records.Add(record);
                }
                var individualEffectsDict = collections?
                    .SingleOrDefault(c => c.Target == target).IndividualEffects;
                if (individualEffectsDict?.Count > 0) {
                    foreach (var substance in substances) {
                        var descriptor = new SubstanceContributorKey() {
                            Substance = substance
                        };
                        if (individualEffectsDict.TryGetValue(substance, out var results)) {
                            var effects = getIndividualEffects(results, null);
                            var record = createSubstanceRiskRecord(target, effects, false, isInverseDistribution, descriptor, null);
                            records.Add(record);
                        }
                    }
                }
                foreach (var item in records) {
                    var recordsLookup = RiskRecords.SingleOrDefault(c => c.Target == target).Records
                        .ToDictionary(r => r.SubstanceCode);
                    var record = recordsLookup[item.SubstanceCode];
                    record.UncertaintyLowerLimit = uncertaintyLowerBound;
                    record.UncertaintyUpperLimit = uncertaintyUpperBound;
                    record.RiskPercentiles.AddUncertaintyValues(new List<double> { item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom });
                    record.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
                }
            }
        }
    }
}
