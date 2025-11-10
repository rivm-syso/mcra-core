namespace MCRA.Simulation.OutputGeneration.CombinedActionSummaries {
    public record CombinedPercentileRecord(
        string IdModel,
        string Name,
        double Percentage,
        double Value,
        double? UncertaintyMedian,
        double? UncertaintyLowerBound,
        double? UncertaintyUpperBound,
        List<double> UncertaintyValues
    ) {
        public bool HasUncertainty =>
            UncertaintyMedian != null
                && UncertaintyLowerBound != null
                && UncertaintyUpperBound != null
                && !double.IsNaN((double)UncertaintyMedian)
                && (!double.IsNaN((double)UncertaintyLowerBound)
                    || !double.IsNaN((double)UncertaintyUpperBound));
    }
}
