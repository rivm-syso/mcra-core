using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleHazardIndexSection : HazardIndexSectionBase {

        /// <summary>
        /// Summarizes hazard indexes single substance.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="focalEffect"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeSingleSubstance(
            List<IndividualEffect> individualEffects,
            Compound substance,
            Effect focalEffect,
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            RiskMetricCalculationType riskMetricCalculationType,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            IsInverseDistribution = isInverseDistribution;
            EffectName = focalEffect?.Name;
            NumberOfSubstances = 1;
            ConfidenceInterval = confidenceInterval;
            ThresholdHazardIndex = threshold;
            HealthEffectType = healthEffectType;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            HiBarPercentages = new double[] { pLower, 50, 100 - pLower };
            HazardIndexRecords = GetHazardIndexSingleRecord(
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            CED = hazardCharacterisations.Distinct().Count() == 1 ? hazardCharacterisations.Average(weights) : double.NaN;
        }

        /// <summary>
        /// Summarizes uncertainty for Hazard Index safety charts single substance.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeSingleSubstanceUncertainty(
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var recordLookup = HazardIndexRecords.ToDictionary(r => r.CompoundCode);
            var hazardIndexRecords = GetHazardIndexSingleRecord(
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            foreach (var item in hazardIndexRecords) {
                var record = recordLookup[item.CompoundCode];
                record.UncertaintyLowerLimit = uncertaintyLowerBound;
                record.UncertaintyUpperLimit = uncertaintyUpperBound;
                record.HazardIndexPercentiles.AddUncertaintyValues(new List<double> { item.PLowerHINom, item.HIP50Nom, item.PUpperHINom });
                record.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
            }
        }
    }
}
