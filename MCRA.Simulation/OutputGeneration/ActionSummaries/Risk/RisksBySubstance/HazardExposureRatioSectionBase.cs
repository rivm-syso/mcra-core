using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HazardExposureRatioSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        public string EffectName { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> RiskRecords { get; set; } = new();
        public double ConfidenceInterval { get; set; }
        public double[] RiskBarPercentages;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }

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
                .Select(c => c.SamplingWeight)
                .ToList();

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositives = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SamplingWeight)
                .Sum();
            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardExposureRatio < Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();

            // TODO: don't compute percentiles here, but use risk percentiles calculated
            // in the action calculator (passed as part of action result).
            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                var risks = individualEffects.Select(c => c.ExposureHazardRatio);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => c == 0 ? _eps : 1 / c).ToList();
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
