using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleHazardExposureRatioSection : HazardExposureRatioSectionBase {

        public List<TargetUnit> TargetUnits { get; set; }

        /// <summary>
        /// Summarizes risk ratio (hazard/exposure) records by substance.
        /// </summary>
        public void Summarize(
            List<TargetUnit> targetUnits,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            double threshold,
            double confidenceInterval,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            EffectName = focalEffect?.Name;
            TargetUnits = targetUnits;
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            RiskBarPercentages = new double[] { pLower, 50, 100 - pLower };

            var cumulativeTarget = targetUnits.Count == 1 ? targetUnits.First().Target : null;
            var orderedTargetUnits = targetUnits.OrderByDescending(r => r?.Target == cumulativeTarget).ToList();
            foreach (var targetUnit in orderedTargetUnits) {
                var target = targetUnit?.Target;
                var targetSummaryRecords = createRisksRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections?
                        .SingleOrDefault(c => c.Target == target).IndividualEffects,
                    individualEffects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative && target == cumulativeTarget
                );
                RiskRecords.Add((target, targetSummaryRecords));
                if (individualEffects != null) {
                    // TODO: refactor this. The individual effects record is not target specific
                    // (unless there is only one target) it should be summarized outside the targets
                    // loop.
                    var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
                    var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
                }
            }
        }

        /// <summary>
        /// Summarizes uncertainty results.
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
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;

            var cumulativeTarget = targetUnits.Count == 1 ? targetUnits.First().Target : null;
            foreach (var targetUnit in targetUnits) {
                var target = targetUnit?.Target;
                var recordsLookup = RiskRecords.SingleOrDefault(c => c.Target == target).Records
                    .ToDictionary(r => r.SubstanceCode);
                var records = createRisksRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections.SingleOrDefault(c => c.Target == target).IndividualEffects,
                    individualEffects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative && target == cumulativeTarget
                );
                foreach (var item in records) {
                    var record = recordsLookup[item.SubstanceCode];
                    record.UncertaintyLowerLimit = uncertaintyLowerBound;
                    record.UncertaintyUpperLimit = uncertaintyUpperBound;
                    record.RiskPercentiles.AddUncertaintyValues(new List<double> { item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom });
                    record.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
                }
            }
        }

        private List<SubstanceRiskDistributionRecord> createRisksRecords(
            ExposureTarget target,
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<SubstanceRiskDistributionRecord>();

            if (substances.Count > 1 && individualEffects != null && isCumulative) {
                Compound riskReference = null;
                if (riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                } else if (riskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
                var record = createSubstanceRiskRecord(target, individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }

            if (individualEffectsBySubstance?.Any() ?? false) {
                foreach (var substance in substances) {
                    if (individualEffectsBySubstance.TryGetValue(substance, out var result)) {
                        var record = createSubstanceRiskRecord(target, result, substance, false, isInverseDistribution);
                        records.Add(record);
                    }
                }
            }

            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }
    }
}
