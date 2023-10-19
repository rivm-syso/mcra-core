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
        public double Threshold { get; set; }
        public List<HistogramBin> RiskDistributionBins { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }

        /// <summary>
        /// Summarizes risks distribution.
        /// </summary>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="individualEffects"></param>
        public abstract void Summarize(
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            List<IndividualEffect> individualEffects
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
