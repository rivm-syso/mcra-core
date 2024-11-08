using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleExposureHazardRatioSection : ExposureHazardRatioSectionBase {

        public List<TargetUnit> TargetUnits { get; set; }

        /// <summary>
        /// Summarizes risk characterisation ratio (exposure/hazard) records by substance.
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
            bool isCumulative,
            bool skipPrivacySensitiveOutputs
        ) {
            EffectName = focalEffect?.Name;
            TargetUnits = targetUnits;
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            //UseIntraSpeciesFactor = useIntraSpeciesFactor;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            RiskBarPercentages = new double[] { pLower, 50, 100 - pLower };
            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            var cumulativeTarget = targetUnits.Count == 1 ? targetUnits.First().Target : null;
            var orderedTargetUnits = targetUnits.OrderByDescending(r => r?.Target == cumulativeTarget).ToList();
            foreach (var targetUnit in orderedTargetUnits) {
                var target = targetUnit?.Target;
                var targetSummaryRecords = GetRiskMultipeRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    isInverseDistribution,
                    isCumulative && target == cumulativeTarget
                );
                RiskRecords.Add((target, targetSummaryRecords));
                if (individualEffects != null) {
                    // TODO: refactor this. The individual effects record is not target specific
                    // (unless there is only one target) it should be summarized outside the targets
                    // loop.
                    //var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
                    //var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
                }
            }
        }

        /// <summary>
        /// Summarizes uncertainty results.
        /// </summary>
        /// <param name="targetUnits"></param>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstanceCollections"></param>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeUncertain(
            ICollection<TargetUnit> targetUnits,
            ICollection<Compound> substances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
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
                var recordsLookup = RiskRecords
                    .SingleOrDefault(c => c.Target == target).Records
                    .ToDictionary(r => r.SubstanceCode);
                var records = GetRiskMultipeRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
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
    }
}
