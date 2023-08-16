using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleThresholdExposureRatioSection : ThresholdExposureRatioSectionBase {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes IMOE safety charts multiple substances
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="threshold"></param>
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
            double threshold,
            double confidenceInterval,
            HealthEffectType healthEffectType,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative,
            bool onlyCumulativeOutput
        ) {
            RiskMetricType = riskMetricType;
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
            RiskBarPercentages = new double[] { pLower, 50, 100 - pLower };
            RiskRecords = createMarginOfExposureRecords(
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
            var recordsLookup = RiskRecords.ToDictionary(r => r.SubstanceCode);
            var records = createMarginOfExposureRecords(
                substances,
                individualEffectsBySubstance,
                individualEffects,
                riskMetricCalculationType,
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

        private List<SubstanceRiskDistributionRecord> createMarginOfExposureRecords(
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
                var record = createSubstanceThresholdExposureRatioRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }

            foreach (var substance in substances) {
                var record = createSubstanceThresholdExposureRatioRecord(individualEffectsBySubstance[substance], substance, false, isInverseDistribution);
                records.Add(record);
            }

            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }
    }
}
