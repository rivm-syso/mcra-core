using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class ThresholdExposureRatioDistributionSection : RisksDistributionSection {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        /// <summary>
        /// Summarizes risks distribution.
        /// </summary>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="individualEffects"></param>
        /// <param name="referenceDose"></param>
        public override void Summarize(
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            bool isInverseDistribution,
            double[] selectedPercentiles,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType
        ) {
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Percentages = selectedPercentiles.Select(c => 100 - c).Reverse().ToArray();
            Threshold = threshold;
            HealthEffectType = healthEffectType;
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);

            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualEffectsPositives = individualEffects.Where(c => c.IsPositive).ToList();
            var risks = individualEffects.Select(c => c.ThresholdExposureRatio).ToList();

            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var exposureThresholdRatios = individualEffects.Select(c => c.ExposureThresholdRatio).ToList();
                PercentilesGrid.ReferenceValues = exposureThresholdRatios
                    .PercentilesWithSamplingWeights(weights, complementPercentage)
                    .Select(c => c == 0 ? _eps : 1 / c);
            } else {
                PercentilesGrid.ReferenceValues = risks.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues);
            }

            PercentageZeros = 100 - 100D * individualEffectsPositives.Sum(c => c.SamplingWeight) / weights.Sum();

            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.ThresholdExposureRatio < Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();

            ProbabilityOfCriticalEffect = 100d * sumWeightsCriticalEffect / weights.Sum();

            var logData = individualEffectsPositives.Select(c => Math.Log10(c.ThresholdExposureRatio)).ToList();
            if (logData.Any()) {
                //Take all intakes for a better resolution
                int numberOfBins = Math.Sqrt(weights.Count) < 100 ? BMath.Ceiling(Math.Sqrt(weights.Count)) : 100;
                var samplingWeights = individualEffectsPositives.Select(c => c.SamplingWeight).ToList();
                RiskDistributionBins = logData.MakeHistogramBins(samplingWeights, numberOfBins, logData.Min(), logData.Max());
            }
        }

        /// <summary>
        /// Summarizes results of an uncertainty run.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public override void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var risks = individualEffects.Select(c => c.ThresholdExposureRatio).ToList();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var exposureThresholdRatios = individualEffects.Select(c => c.ExposureThresholdRatio).ToList();
                PercentilesGrid.AddUncertaintyValues(exposureThresholdRatios
                    .PercentilesWithSamplingWeights(weights, complementPercentage)
                    .Select(c => c == 0 ? _eps : 1 / c));
            } else {
                PercentilesGrid.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
            }
        }
    }
}
