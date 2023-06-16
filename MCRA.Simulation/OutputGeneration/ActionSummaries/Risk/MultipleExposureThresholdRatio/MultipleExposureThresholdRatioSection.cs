using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleExposureThresholdRatioSection : ExposureThresholdRatioSectionBase {

        /// <summary>
        /// Summarizes exposure/threshold value by substance.
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        /// <param name="onlyCumulativeOutput"></param>
        public void SummarizeMultipleSubstances(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool useIntraSpeciesFactor,
            bool isCumulative,
            bool onlyCumulativeOutput
        ) {
            UseIntraSpeciesFactor = useIntraSpeciesFactor;
            OnlyCumulativeOutput = onlyCumulativeOutput;
            IsInverseDistribution = isInverseDistribution;
            EffectName = focalEffect?.Name;
            NumberOfSubstances = substances.Count;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            HealthEffectType = healthEffectType;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            HiBarPercentages = new double[] { pLower, 50, 100 - pLower };
            ExposureThresholdRatioRecords = GetRiskMultipeRecords(
                substances,
                individualEffectsBySubstance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            if (individualEffects != null) {
                var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
                var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
                CED = hazardCharacterisations.Distinct().Count() == 1 ? hazardCharacterisations.Average(weights) :double.NaN;
            }
        }

        /// <summary>
        /// Summarizes uncertainty for Exposure/threshold value safety charts.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeMultipleSubstancesUncertainty(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var recordLookup = ExposureThresholdRatioRecords.ToDictionary(r => r.CompoundCode);
            var riskRecords = GetRiskMultipeRecords(
                substances,
                individualEffectsBySubstance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            foreach (var item in riskRecords) {
                var record = recordLookup[item.CompoundCode];
                record.UncertaintyLowerLimit = uncertaintyLowerBound;
                record.UncertaintyUpperLimit = uncertaintyUpperBound;
                record.ExposureRisks.AddUncertaintyValues(new List<double> { item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom });
                record.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
            }
        }
    }
}
