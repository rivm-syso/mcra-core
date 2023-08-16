using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RisksDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public double ProbabilityOfCriticalEffect { get; set; }
        public double PercentageZeros { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] Percentages;
        public double Threshold { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public List<HistogramBin> RiskDistributionBins { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public bool IsInverseDistribution { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public RiskMetricType RiskMetricType { get; set; }

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
        /// <param name="riskMetricType"></param>
        /// <param name="riskMetricCalculationType"></param>
        public abstract void Summarize(
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            bool isInverseDistribution,
            double[] selectedPercentiles,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType
        );

        /// <summary>
        /// Summarizes results of an uncertainty run.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public abstract void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        );
    }
}
