namespace MCRA.Simulation.OutputGeneration {
    public sealed class CombinedRiskPercentileRecord {
        public string IdModel { get; set; }
        public string Name { get; set; }
        public double Percentage { get; set; }
        public double Risk { get; set; }
        public double? UncertaintyMedian { get; set; }
        public double? UncertaintyLowerBound { get; set; }
        public double? UncertaintyUpperBound { get; set; }

        public List<double> UncertaintyValues { get; set; }

        public bool HasUncertainty =>
            UncertaintyMedian != null
                && UncertaintyLowerBound != null
                && UncertaintyUpperBound != null
                && !double.IsNaN((double)UncertaintyMedian)
                && (!double.IsNaN((double)UncertaintyLowerBound)
                    || !double.IsNaN((double)UncertaintyUpperBound));
    }
}
