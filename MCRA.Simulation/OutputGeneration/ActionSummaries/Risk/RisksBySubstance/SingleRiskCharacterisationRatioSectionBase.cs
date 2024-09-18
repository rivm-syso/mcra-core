using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class SingleRiskCharacterisationRatioSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        public string EffectName { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] RiskBarPercentages;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }

        public TargetUnit TargetUnit { get; set; }
        public double ReferenceDose { get; set; }
        public SubstanceRiskDistributionRecord RiskRecord { get; set; }


        public double? RestrictedUpperPercentile { get; set; }

        /// <summary>
        /// Summarizes uncertainty for risks safety charts single substance.
        /// </summary>
        public void Summarize(
            List<IndividualEffect> individualEffects,
            TargetUnit targetUnit,
            Compound substance,
            Effect focalEffect,
            double confidenceInterval,
            double threshold,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative,
            bool skipPrivacySensitiveOutputs
        ) {
            var pLower = (100 - confidenceInterval) / 2;
            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                    pUpper = maxUpperPercentile;
                    pLower = 100 - pUpper;
                    confidenceInterval = 100 - 2 * pLower;
                }
            }
            EffectName = focalEffect?.Name;
            TargetUnit = targetUnit;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            ReferenceDose = referenceDose?.Value ?? double.NaN;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            RiskBarPercentages = new double[] { pLower, 50, pUpper };
            RiskRecord = getSingleRiskRecord(
                targetUnit?.Target,
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
        }

        /// <summary>
        /// Summarizes uncertainty for risks safety charts single substance.
        /// </summary>
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
            var item = getSingleRiskRecord(
                new ExposureTarget(),
                substance,
                individualEffects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative
            );
            RiskRecord.UncertaintyLowerLimit = uncertaintyLowerBound;
            RiskRecord.UncertaintyUpperLimit = uncertaintyUpperBound;
            RiskRecord.RiskPercentiles.AddUncertaintyValues(new List<double> { item.PLowerRiskNom, item.RiskP50Nom, item.PUpperRiskNom });
            RiskRecord.ProbabilityOfCriticalEffects.AddUncertaintyValues(new List<double> { item.ProbabilityOfCriticalEffect });
        }

        protected SubstanceRiskDistributionRecord getSingleRiskRecord(
            ExposureTarget target,
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
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
                return record;
            } else {
                var record = createSubstanceRiskRecord(target, individualEffects, substance, false, isInverseDistribution);
                return record;
            }
        }

        protected SubstanceRiskDistributionRecord createSubstanceRiskRecord(
            ExposureTarget target,
            List<IndividualEffect> individualEffects,
            Compound substance,
            bool isCumulativeRecord,
            bool isInverseDistribution
        ) {
            var allWeights = individualEffects
                .Select(c => c.SamplingWeight)
                .ToList();

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositives = individualEffects
                .Where(c => c.IsPositive)
                .Sum(c => c.SamplingWeight);
            var sumWeightsCriticalEffect = (RiskMetricType == RiskMetricType.ExposureHazardRatio)
                ? individualEffects
                    .Where(c => c.ExposureHazardRatio > Threshold)
                    .Sum(c => c.SamplingWeight)
                : individualEffects
                    .Where(c => c.HazardExposureRatio < Threshold)
                    .Sum(c => c.SamplingWeight);

            var percentiles = new List<double>();
            if (RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                if (isInverseDistribution) {
                    var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                    var risks = individualEffects.Select(c => c.HazardExposureRatio);
                    percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                        .Select(c => 1 / c).ToList();
                } else {
                    percentiles = individualEffects.Select(c => c.ExposureHazardRatio)
                        .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)
                        .ToList();
                }
            } else {
                if (isInverseDistribution) {
                    var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                    var risks = individualEffects.Select(c => c.ExposureHazardRatio);
                    percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                        .Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c).ToList();
                } else {
                    percentiles = individualEffects.Select(c => c.HazardExposureRatio)
                        .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)
                        .ToList();
                }
            }

            var result = new SubstanceRiskDistributionRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target?.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                IsCumulativeRecord = isCumulativeRecord,
                NumberOfIndividuals = individualEffects.Count,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { Threshold },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffect / sumAllWeights },
                },
                RiskPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = RiskBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return result;
        }
    }
}
