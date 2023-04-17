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
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
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
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            double thresholdMarginOfExposure,
            double confidenceInterval,
            HealthEffectType healthEffectType,
            RiskMetricCalculationType riskMetricCalculationType,
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
            MoeRecords = createMarginOfExposureRecords(
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
                CED = hazardCharacterisations.Distinct().Count() == 1 ? hazardCharacterisations.Average(weights) : double.NaN;
            }
        }

        /// <summary>
        /// Summarizes uncertainty for IMOE safety charts multiple substances.
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
            var recordsLookup = MoeRecords.ToDictionary(r => r.CompoundCode);
            var records = createMarginOfExposureRecords(
                substances,
                individualEffectsBySubstance,
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

        private List<MarginOfExposureRecord> createMarginOfExposureRecords(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<MarginOfExposureRecord>();

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
                var record = createSubstanceMarginOfExposureRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }

            foreach (var substance in substances) {
                var record = createSubstanceMarginOfExposureRecord(individualEffectsBySubstance[substance], substance, false, isInverseDistribution);
                records.Add(record);
            }

            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }
    }
}
