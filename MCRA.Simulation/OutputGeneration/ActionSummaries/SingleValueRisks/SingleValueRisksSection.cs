using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks {
    public sealed class SingleValueRisksSection : ActionSummaryBase {

        public RiskMetricType RiskMetric { get; set; }
        public SingleValueSummaryRecord Record { get; set; }
        public bool IsInversDistribution { get; set; }
        public double Percentage { get; set; }
        public bool IsAdjustment { get; set; }
        public double UncertaintyLowerBound { get; set; }
        public double UncertaintyUpperBound { get; set; }

        public void Summarize(
                bool isInversDistribution,
                double percentage,
                SingleValueSummaryRecord summaryRecord,
                RiskMetricType riskMetric,
                bool isAdjustment,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            UncertaintyUpperBound = uncertaintyUpperBound;
            UncertaintyLowerBound = uncertaintyLowerBound;
            IsInversDistribution = isInversDistribution;
            RiskMetric = riskMetric;
            if (RiskMetric == RiskMetricType.MarginOfExposure) {
                Percentage = percentage;
            } else {
                Percentage = 100 - percentage;
            }
            Record = summaryRecord;
            IsAdjustment = isAdjustment;
        }
    }
}
