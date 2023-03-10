using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskPercentile {
        public double Percentage { get; set; }
        public double Risk { get; set; }
        public List<double> RiskUncertainties { get; set; }
    }
}

