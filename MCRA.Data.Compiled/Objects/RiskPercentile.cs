namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskPercentile {
        public double Percentage { get; set; }
        public double MarginOfExposure { get; set; }
        public List<double> MarginOfExposureUncertainties { get; set; }
    }
}
