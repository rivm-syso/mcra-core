using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskCharacterisationRatioSectionBase<S, T1> : SummarySection
        where S : IContributorKey, new()
        where T1 : RiskDistributionRecordBase<S>, new() {

        public override bool SaveTemporaryData => true;

        public List<OutputStratificationVariable> StratificationVariables = [];

        public RiskMetricType RiskMetric { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public string EffectName { get; set; }
        public bool IsInverseDistribution { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] RiskBarPercentages {get; set; } = [];
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double ReferenceDose { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        protected T1 createSubstanceRiskRecord(
            ExposureTarget target,
            ICollection<IndividualEffectsByStratificationLevel> individualEffects,
            bool isCumulativeRecord,
            bool isInverseDistribution,
            S descriptor,
            IStratificationLevel stratificationLevel
        ) {
            var allWeights = individualEffects
                .Select(c => c.SamplingWeight)
                .ToList();

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositives = individualEffects
                .Where(c => c.IsPositive)
                .Sum(c => c.SamplingWeight);
            var sumWeightsCriticalEffect = (RiskMetric == RiskMetricType.ExposureHazardRatio)
                ? individualEffects
                    .Where(c => c.ExposureHazardRatio > Threshold)
                    .Sum(c => c.SamplingWeight)
                : individualEffects
                    .Where(c => c.HazardExposureRatio < Threshold)
                    .Sum(c => c.SamplingWeight);

            var percentiles = new List<double>();
            if (RiskMetric == RiskMetricType.ExposureHazardRatio) {
                if (isInverseDistribution) {
                    var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                    var risks = individualEffects.Select(c => c.HazardExposureRatio);
                    percentiles = [.. risks.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                        .Select(c => 1 / c)];
                } else {
                    percentiles = [.. individualEffects.Select(c => c.ExposureHazardRatio)
                        .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)];
                }
            } else {
                if (isInverseDistribution) {
                    var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                    var risks = individualEffects.Select(c => c.ExposureHazardRatio);
                    percentiles = [.. risks.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                        .Select(c => c == 0 ? double.PositiveInfinity : 1 / c)];
                } else {
                    percentiles = [.. individualEffects.Select(c => c.HazardExposureRatio)
                        .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)];
                }
            }

            var result = new T1() {
                Stratification = stratificationLevel?.Name,
                StratificationLevelCode = stratificationLevel?.Code,
                StratificationSubLevelCodes = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Code).ToList() ?? [stratificationLevel?.Code],
                StratificationSubLevelNames = (stratificationLevel as CompositeStratificationLevel)?.Parts.Select(c => c.Name).ToList() ?? [stratificationLevel?.Name],

                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target?.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                IsCumulativeRecord = isCumulativeRecord,
                NumberOfIndividuals = individualEffects.Count,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = [Threshold],
                    ReferenceValues = [100d * sumWeightsCriticalEffect / sumAllWeights],
                },
                RiskPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = RiskBarPercentages,
                    ReferenceValues = [percentiles[0], percentiles[1], percentiles[2]],
                },
            };
            result.SetDescriptorValues(descriptor);
            return result;
        }


        protected List<IndividualEffectsByStratificationLevel> getIndividualEffects(
            List<IndividualEffect> individualEffects,
            PopulationStratifier stratifier
        ) {
            return individualEffects
                .Select(c =>
                    new IndividualEffectsByStratificationLevel {
                        SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                        HazardExposureRatio = c.HazardExposureRatio,
                        ExposureHazardRatio = c.ExposureHazardRatio,
                        StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual),
                        IsPositive = c.IsPositive
                    })
                .ToList();
        }

        protected static Compound getRiskReference(RiskMetricCalculationType riskMetricCalculationType) {
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
            return riskReference;
        }
    }
}
