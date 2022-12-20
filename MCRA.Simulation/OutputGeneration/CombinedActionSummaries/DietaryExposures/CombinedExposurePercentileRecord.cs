namespace MCRA.Simulation.OutputGeneration {
    public sealed class CombinedExposurePercentileRecord {
        public string IdModel { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
        public double? UncertaintyMedian { get; set; }
        public double? UncertaintyLowerBound { get; set; }
        public double? UncertaintyUpperBound { get; set; }

        public bool HasUncertainty() {
            return UncertaintyMedian != null
                && UncertaintyLowerBound != null
                && UncertaintyUpperBound != null
                && !double.IsNaN((double)UncertaintyMedian)
                && (!double.IsNaN((double)UncertaintyLowerBound)
                    || !double.IsNaN((double)UncertaintyUpperBound));
        }
    }
}
