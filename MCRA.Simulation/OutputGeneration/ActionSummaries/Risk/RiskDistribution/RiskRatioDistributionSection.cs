using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskRatioDistributionSection : RisksDistributionSection {
        public override bool SaveTemporaryData => true;
        public void Summarize(
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            List<IndividualEffect> individualEffects,
            RiskMetricType riskMetricType
        ) {
            RiskMetricType = riskMetricType;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            if (riskMetricType == RiskMetricType.HazardExposureRatio) {
                summarizeHazardExposure(
                    confidenceInterval,
                    threshold,
                    isInverseDistribution,
                    individualEffects
                );
            } else {
                summarizeExposureHazard(
                    confidenceInterval,
                    threshold,
                    isInverseDistribution,
                    individualEffects
                );
            }
        }

        public void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            RiskMetricType riskMetricType
        ) {
            if (riskMetricType == RiskMetricType.HazardExposureRatio) {
                summarizeUncertaintyHazardExposure(
                    individualEffects,
                    isInverseDistribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
            } else {
                summarizeUncertaintyExposureHazard(
                         individualEffects,
                         isInverseDistribution,
                         uncertaintyLowerBound,
                         uncertaintyUpperBound
                     );
            }
        }


        /// <summary>
        /// Summarizes risks distribution.
        /// </summary>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="individualEffects"></param>
        private void summarizeHazardExposure(
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            List<IndividualEffect> individualEffects
        ) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualEffectsPositives = individualEffects.Where(c => c.IsPositive).ToList();
            var risks = individualEffects.Select(c => c.HazardExposureRatio).ToList();

            PercentilesGrid = new UncertainDataPointCollection<double> {
                XValues = GriddingFunctions.GetPlotPercentages()
            };
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                PercentilesGrid.ReferenceValues = exposureHazardRatios
                    .PercentilesWithSamplingWeights(weights, complementPercentage)
                    .Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c);
            } else {
                PercentilesGrid.ReferenceValues = risks.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues);
            }

            PercentageZeros = 100 - 100D * individualEffectsPositives.Sum(c => c.SamplingWeight) / weights.Sum();

            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardExposureRatio < Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();

            ProbabilityOfCriticalEffect = 100d * sumWeightsCriticalEffect / weights.Sum();

            var logData = individualEffectsPositives.Select(c => Math.Log10(c.HazardExposureRatio)).ToList();
            if (logData.Any()) {
                //Take all intakes for a better resolution
                int numberOfBins = Math.Sqrt(weights.Count) < 100 ? BMath.Ceiling(Math.Sqrt(weights.Count)) : 100;
                var samplingWeights = individualEffectsPositives.Select(c => c.SamplingWeight).ToList();
                RiskDistributionBins = logData.MakeHistogramBins(samplingWeights, numberOfBins, logData.Min(), logData.Max());
            }
        }

        public void summarizeExposureHazard(
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            List<IndividualEffect> individualEffects
        ) {
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
        private void summarizeUncertaintyHazardExposure(
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var risks = individualEffects.Select(c => c.HazardExposureRatio).ToList();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                PercentilesGrid.AddUncertaintyValues(exposureHazardRatios
                    .PercentilesWithSamplingWeights(weights, complementPercentage)
                    .Select(c => c == 0 ? SimulationConstants.MOE_eps : 1 / c));
            } else {
                PercentilesGrid.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
            }
        }

        private void summarizeUncertaintyExposureHazard(
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
