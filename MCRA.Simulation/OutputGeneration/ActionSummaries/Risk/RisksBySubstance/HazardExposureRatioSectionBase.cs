﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HazardExposureRatioSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public string EffectName { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> RiskRecords { get; set; } = [];
        public double ConfidenceInterval { get; set; }
        public double[] RiskBarPercentages;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        /// <summary>
        /// Calculate IMOE statistics per substances.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="isCumulativeRecord"></param>
        /// <param name="isInverseDistribution"></param>
        /// <returns></returns>
        protected SubstanceRiskDistributionRecord createSubstanceRiskRecord(
            ExposureTarget target,
            List<IndividualEffect> individualEffects,
            Compound substance,
            bool isCumulativeRecord,
            bool isInverseDistribution
        ) {
            var allWeights = individualEffects
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositives = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .Sum();
            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardExposureRatio < Threshold)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .Sum();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                var risks = individualEffects.Select(c => c.ExposureHazardRatio);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.HazardExposureRatio)
                    .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)
                    .ToList();
            }

            var result = new SubstanceRiskDistributionRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                IsCumulativeRecord = isCumulativeRecord,
                NumberOfIndividuals = individualEffects.Count,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = [Threshold],
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
