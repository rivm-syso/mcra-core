using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureHazardRatioSection : ExposureHazardRatioSectionBase {
        public double UpperBoundConficenceInterval { get ; set; }   

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
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            RiskBarPercentages = new double[] { pLower, 50, 100 - pLower };
            UpperBoundConficenceInterval = RiskBarPercentages[2];

            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            var selectedTargets = targetUnits.Where(c => c != null).ToList();
            foreach (var targetUnit in selectedTargets) {
                var target = targetUnit?.Target;
                RiskRecords.Add((target, GetRiskMultipeRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    isInverseDistribution,
                    isCumulative)
                ));
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
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            foreach (var target in targets) {
                var recordsLookup = RiskRecords.SingleOrDefault(c => c.Target == target).Records
                    .ToDictionary(r => r.SubstanceCode);
                var records = GetRiskMultipeRecords(
                    target,
                    substances,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    isInverseDistribution,
                    isCumulative
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
