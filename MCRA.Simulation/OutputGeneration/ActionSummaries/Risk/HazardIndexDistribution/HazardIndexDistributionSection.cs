using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardIndexDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 1 / 10E7D;
        public double ProbabilityOfCriticalEffect { get; set; }
        public double PercentageZeros { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] Percentages;
        public double ThresholdHazardIndex { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public List<HistogramBin> HIDistributionBins { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public bool IsInverseDistribution { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        /// <summary>
        /// summarizes Hazard Index distribution cumulative substance.
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
            Percentages = selectedPercentiles;
            ThresholdHazardIndex = threshold;
            HealthEffectType = healthEffectType;
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);

            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var hazardIndices = individualEffects.Select(c => c.HazardIndex).ToList();
            var individualEffectsPositives = individualEffects.Where(c => c.IsPositive).ToList();

            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();

            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var marginOfExposures = individualEffects.Select(c => c.MarginOfExposure).ToList();
                PercentilesGrid.ReferenceValues = marginOfExposures.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => c == 1 / _eps ? _eps : 1 / c);
            } else {
                PercentilesGrid.ReferenceValues = hazardIndices.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues).Select(c => c == 0 ? _eps : c);
            }

            PercentageZeros = 100 - 100D * individualEffectsPositives.Sum(c => c.SamplingWeight) / weights.Sum();

            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardIndex > ThresholdHazardIndex)
                .Select(c => c.SamplingWeight)
                .Sum();

            ProbabilityOfCriticalEffect = 100d * sumWeightsCriticalEffect / weights.Sum();

            var logData = individualEffectsPositives.Select(c => Math.Log10(c.HazardIndex)).ToList();
            if (logData.Any()) {
                //Take all intakes for a better resolution
                var numberOfBins = Math.Sqrt(weights.Count) < 100 ? BMath.Ceiling(Math.Sqrt(weights.Count)) : 100;
                var samplingWeights = individualEffectsPositives.Select(c => c.SamplingWeight).ToList();
                HIDistributionBins = logData.MakeHistogramBins(samplingWeights, numberOfBins, logData.Min(), logData.Max());
            }
        }
        
        /// <summary>
        /// Summarizes uncertainty for HI distribution
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                List<IndividualEffect> individualEffects,
                bool isInverseDistribution,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var hazardIndices = individualEffects.Select(c => c.HazardIndex).ToList();
            if (isInverseDistribution) {
                var complementPercentage = PercentilesGrid.XValues.Select(c => 100 - c);
                var marginsOfExposure = individualEffects.Select(c => c.MarginOfExposure).ToList();
                PercentilesGrid.AddUncertaintyValues(marginsOfExposure.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => c == 1 / _eps ? _eps : 1 / c));
            } else {
                PercentilesGrid.AddUncertaintyValues(hazardIndices.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues).Select(c => c == 0 ? _eps : c));
            }
        }
    }
}
