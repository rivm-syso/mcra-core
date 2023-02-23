using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleMarginOfExposureSection : MarginOfExposureSectionBase {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes IMOE safety charts multiple substances
        /// </summary>
        /// <param name="substanceIndividualEffects"></param>
        /// <param name="cumulativeIndividualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="thresholdMarginOfExposure"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        /// <param name="onlyCumulativeOutput"></param>
        public void SummarizeMultipleSubstances(
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            List<IndividualEffect> cumulativeIndividualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            double thresholdMarginOfExposure,
            double confidenceInterval,
            HealthEffectType healthEffectType,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative,
            bool onlyCumulativeOutput
        ) {
            OnlyCumulativeOutput = onlyCumulativeOutput;
            IsInverseDistribution = isInverseDistribution;
            EffectName = focalEffect?.Name;
            NumberOfSubstances = substances.Count;
            ConfidenceInterval = confidenceInterval;
            ThresholdMarginOfExposure = thresholdMarginOfExposure;
            HealthEffectType = healthEffectType;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            MoeBarPercentages = new double[] { pLower, 50, 100 - pLower };
            MOERecords = GetMarginOfExposureMultipleRecords(
                substances,
                substanceIndividualEffects,
                cumulativeIndividualEffects,
                isInverseDistribution,
                isCumulative
            );
            if (cumulativeIndividualEffects != null) {
                var weights = cumulativeIndividualEffects.Select(c => c.SamplingWeight).ToList();
                var hazardCharacterisations = cumulativeIndividualEffects.Select(c => c.CriticalEffectDose).ToList();
                CED = hazardCharacterisations.Distinct().Count() == 1 ? hazardCharacterisations.Average(weights) : double.NaN;
            }
        }

        /// <summary>
        /// Summarizes uncertainty for IMOE safety charts multiple substances.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsDict"></param>
        /// <param name="cumulativeIndividualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeMultipleSubstancesUncertainty(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsDict,
            List<IndividualEffect> cumulativeIndividualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var recordsLookup = MOERecords.ToDictionary(r => r.CompoundCode);
            var records = GetMarginOfExposureMultipleRecords(
                substances,
                individualEffectsDict,
                cumulativeIndividualEffects,
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
