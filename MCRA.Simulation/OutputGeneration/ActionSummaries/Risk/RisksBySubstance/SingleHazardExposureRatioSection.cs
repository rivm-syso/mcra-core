using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleHazardExposureRatioSection : HazardExposureRatioSectionBase {

        public TargetUnit TargetUnit { get; set; }

        public double ReferenceDose { get; set; }

        public SubstanceRiskDistributionRecord RiskRecord => RiskRecords.Single().Records.Single();

        /// <summary>
        /// Summarizes risks single substance.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="targetUnit"></param>
        /// <param name="substance"></param>
        /// <param name="focalEffect"></param>
        /// <param name="threshold"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="riskMetricType"></param>
        /// <param name="riskMetricCalculationType"></param>
        /// <param name="referenceDose"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        public void Summarize(
            List<IndividualEffect> individualEffects,
            TargetUnit targetUnit,
            Compound substance,
            Effect focalEffect,
            double threshold,
            double confidenceInterval,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            EffectName = focalEffect?.Name;
            TargetUnit = targetUnit;
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            ReferenceDose = referenceDose?.Value ?? double.NaN;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            var pLower = (100 - ConfidenceInterval) / 2;
            RiskBarPercentages = new double[] { pLower, 50, 100 - pLower };
            var riskRecords = getSingleRiskRecord(
                targetUnit?.Target,
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            RiskRecords.Add((targetUnit?.Target, riskRecords));
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
        }

        /// <summary>
        /// Summarizes uncertainty results.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isCumulative"></param>
        public void SummarizeUncertain(
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
            var recordsLookup = RiskRecords.Single().Records
                .ToDictionary(r => r.SubstanceCode);
            var records = getSingleRiskRecord(
                new ExposureTarget(),
                substance,
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

        private List<SubstanceRiskDistributionRecord> getSingleRiskRecord(
            ExposureTarget target,
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<SubstanceRiskDistributionRecord>();
            if (individualEffects != null && isCumulative) {
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
            } else {
                records.Add(createSubstanceRiskRecord(target, individualEffects, substance, false, isInverseDistribution));
            }
            return records;
        }
    }
}
