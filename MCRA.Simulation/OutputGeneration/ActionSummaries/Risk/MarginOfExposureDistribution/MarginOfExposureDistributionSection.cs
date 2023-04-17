using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class MarginOfExposureDistributionSection : SummarySection {

        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;
        public double ProbabilityOfCriticalEffect { get; set; }
        public double PercentageZeros { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] Percentages;
        public double ThresholdMarginOfExposure { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public List<HistogramBin> IMOEDistributionBins { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public bool IsInverseDistribution { get; set; }

        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        /// <summary>
        /// Summarizes IMOE distribution cumulative substance.
        /// </summary>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="individualEffects"></param>
        /// <param name="referenceDose"></param>
        public void Summarize(
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            bool isInverseDistribution,
            double[] selectedPercentiles,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            RiskMetricCalculationType riskMetricCalculationType
        ) {
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            Percentages = selectedPercentiles.Select(c => 100 - c).Reverse().ToArray();
            ThresholdMarginOfExposure = threshold;
            HealthEffectType = healthEffectType;
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);

            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualEffectsPositives = individualEffects.Where(c => c.IsPositive).ToList();
            var marginsOfExposure = individualEffects.Select(c => c.MarginOfExposure).ToList();

            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var hazardIndices = individualEffects.Select(c => c.HazardIndex).ToList();
                PercentilesGrid.ReferenceValues = hazardIndices.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => c == 0 ? _eps : 1 / c);
            } else {
                PercentilesGrid.ReferenceValues = marginsOfExposure.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues);
            }

            PercentageZeros = 100 - 100D * individualEffectsPositives.Sum(c => c.SamplingWeight) / weights.Sum();

            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.MarginOfExposure < ThresholdMarginOfExposure)
                .Select(c => c.SamplingWeight)
                .Sum();

            ProbabilityOfCriticalEffect = 100d * sumWeightsCriticalEffect / weights.Sum();

            var logData = individualEffectsPositives.Select(c => Math.Log10(c.MarginOfExposure)).ToList();
            if (logData.Any()) {
                //Take all intakes for a better resolution
                int numberOfBins = Math.Sqrt(weights.Count) < 100 ? BMath.Ceiling(Math.Sqrt(weights.Count)) : 100;
                var samplingWeights = individualEffectsPositives.Select(c => c.SamplingWeight).ToList();
                IMOEDistributionBins = logData.MakeHistogramBins(samplingWeights, numberOfBins, logData.Min(), logData.Max());
            }
        }

        /// <summary>
        /// Summarizes uncertainty for IMOE distribution
        /// </summary>
        /// <param name="individualEffects"></param>
        public void SummarizeUncertainty(
                List<IndividualEffect> individualEffects,
                bool isInverseDistribution,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var marginsOfExposure = individualEffects.Select(c => c.MarginOfExposure).ToList();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var hazardIndices = individualEffects.Select(c => c.HazardIndex).ToList();
                PercentilesGrid.AddUncertaintyValues(hazardIndices.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => c == 0 ? _eps : 1 / c));
            } else {
                PercentilesGrid.AddUncertaintyValues(marginsOfExposure.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
            }
        }
    }
}
