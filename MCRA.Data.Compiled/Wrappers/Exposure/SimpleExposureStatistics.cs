using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Wrappers.Exposure {
    public sealed class SimpleExposureStatistics {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<double> Intakes { get; set; }
        public List<double> SamplingWeights { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit TargetUnit { get; set; }
    }
}
