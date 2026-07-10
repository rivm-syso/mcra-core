using MCRA.General;
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
        public RiskMetricType RiskMetricType { get; set; }
    }
}
