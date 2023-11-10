using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureHazardRatioDistributionSection : RisksDistributionSection {
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// summarizes risks distribution.
        /// </summary>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="individualEffects"></param>
        public override void Summarize(
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            List<IndividualEffect> individualEffects
        ) {
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualEffectsPositives = individualEffects.Where(c => c.IsPositive).ToList();
            var risks = individualEffects.Select(c => c.ExposureHazardRatio).ToList();

            PercentilesGrid = new UncertainDataPointCollection<double> {
                XValues = GriddingFunctions.GetPlotPercentages()
            };
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var hazardExposureRatios = individualEffects.Select(c => c.HazardExposureRatio).ToList();
                PercentilesGrid.ReferenceValues = hazardExposureRatios
                    .PercentilesWithSamplingWeights(weights, complementPercentage)
                    .Select(c => c == SimulationConstants.MOE_eps ? 1 / SimulationConstants.MOE_eps : 1 / c);
            } else {
                PercentilesGrid.ReferenceValues = risks
                    .PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues)
                    .Select(c => c == 0 ? 1 / SimulationConstants.MOE_eps : c);
            }

            PercentageZeros = 100 - 100D * individualEffectsPositives.Sum(c => c.SamplingWeight) / weights.Sum();

            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.ExposureHazardRatio > Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();

            ProbabilityOfCriticalEffect = 100d * sumWeightsCriticalEffect / weights.Sum();

            var logData = individualEffectsPositives.Select(c => Math.Log10(c.ExposureHazardRatio)).ToList();
            if (logData.Any()) {
                //Take all intakes for a better resolution
                var numberOfBins = Math.Sqrt(weights.Count) < 100 ? BMath.Ceiling(Math.Sqrt(weights.Count)) : 100;
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
            var risks = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var hazardExposureRatio = individualEffects.Select(c => c.HazardExposureRatio).ToList();
                PercentilesGrid.AddUncertaintyValues(hazardExposureRatio.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => c == SimulationConstants.MOE_eps ? 1 / SimulationConstants.MOE_eps : 1 / c));
            } else {
                PercentilesGrid.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues).Select(c => c == 0 ? 1 / SimulationConstants.MOE_eps : c));
            }
        }
    }
}
