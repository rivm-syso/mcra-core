using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureHazardRatioSection : ExposureHazardRatioSectionBase<RiskSubstanceContributorKey, SubstanceRiskDistributionRecord> {
        public double UpperBoundConficenceInterval { get; set; }

        public List<TargetUnit> TargetUnits { get; set; }

        /// <summary>
        /// Summarizes risks by substance.
        /// </summary>
        /// <param name="targetUnits"></param>
        /// <param name="individualEffectsBySubstanceCollections"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="riskMetricCalculationType"></param>
        /// <param name="riskMetricType"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        ///
        public void Summarize(
            List<TargetUnit> targetUnits,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
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
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            var selectedTargets = targetUnits.Where(c => c != null).ToList();
            foreach (var target in selectedTargets) {
                var records = new List<SubstanceRiskDistributionRecord>();
                if (substances.Count > 1 && individualEffects != null && isCumulative) {
                    var descriptor = new RiskSubstanceContributorKey() {
                        Substance = getRiskReference(RiskMetricCalculationType)
                    };
                    var effects = getIndividualEffects(individualEffects, null);
                    var record = createSubstanceRiskRecord(target?.Target, effects, true, isInverseDistribution, descriptor, null);
                    records.Add(record);
                }
                var individualEffectsDict = individualEffectsBySubstanceCollections?
                    .SingleOrDefault(c => c.Target == target?.Target).IndividualEffects;
                if (individualEffectsDict?.Count > 0) {
                    foreach (var substance in substances) {
                        var descriptor = new RiskSubstanceContributorKey() {
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

        /// <summary>
        /// Summarizes uncertainty for risk safety charts.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeUncertain(
            ICollection<ExposureTarget> targets,
            ICollection<Compound> substances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> collections,
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            foreach (var target in targets) {
                var records = new List<SubstanceRiskDistributionRecord>();
                if (substances.Count > 1 && individualEffects != null && isCumulative) {
                    var descriptor = new RiskSubstanceContributorKey() {
                        Substance = getRiskReference(RiskMetricCalculationType)
                    };
                    var effects = getIndividualEffects(individualEffects, null);
                    var record = createSubstanceRiskRecord(target, effects, true, isInverseDistribution, descriptor, null);
                    records.Add(record);
                }
                var individualEffectsDict = collections?
                    .SingleOrDefault(c => c.Target == target).IndividualEffects;
                if (individualEffectsDict?.Count > 0) {
                    foreach (var substance in substances) {
                        var descriptor = new RiskSubstanceContributorKey() {
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
