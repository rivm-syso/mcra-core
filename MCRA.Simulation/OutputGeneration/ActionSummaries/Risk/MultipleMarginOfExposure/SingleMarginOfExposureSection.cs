using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleMarginOfExposureSection : MarginOfExposureSectionBase {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes IMOE safety charts single substance.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="focalEffect"></param>
        /// <param name="thresholdMarginOfExposure"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeSingleSubstance(
            List<IndividualEffect> individualEffects,
            Compound substance,
            Effect focalEffect,
            double thresholdMarginOfExposure,
            double confidenceInterval,
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
            ThresholdMarginOfExposure = thresholdMarginOfExposure;
            HealthEffectType = healthEffectType;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            MoeBarPercentages = new double[] { pLower, 50, 100 - pLower };
            MoeRecords = GetMarginOfExposureSingleRecord(
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
        /// Summarizes uncertainty for IMOE safety charts single substance.
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
            var recordsLookup = MoeRecords.ToDictionary(r => r.CompoundCode);
            var records = GetMarginOfExposureSingleRecord(
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            foreach (var item in records) {
                var record = recordsLookup[item.CompoundCode];
                record.UncertaintyLowerLimit = uncertaintyLowerBound;
                record.UncertaintyUpperLimit = uncertaintyUpperBound;
                record.MarginOfExposurePercentiles.AddUncertaintyValues(new List<double> { item.PLowerMOENom, item.MOEP50Nom, item.PUpperMOENom });
                record.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
            }
        }
    }
}
