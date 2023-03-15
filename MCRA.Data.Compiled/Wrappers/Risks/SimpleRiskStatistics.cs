using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Wrappers.Risks {
    public sealed class SimpleRiskStatistics {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<double> Risks { get; set; }
        public List<double> SamplingWeights { get; set; }
        public Compound Substance { get; set; }
        public RiskMetricType RiskMetric { get; set; }
    }
}
