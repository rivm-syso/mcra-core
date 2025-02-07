namespace MCRA.Simulation.OutputGeneration {
    public abstract class ChronicDrilldownSectionBase(
        OverallIndividualDrillDownRecord drilldownRecord,
        int drilldownIndex,
        bool isCumulative,
        bool isProcessing,
        string referenceCompoundName,
        double bodyWeight
    ) : SummarySection {
        public OverallIndividualDrillDownRecord DrilldownRecord { get; } = drilldownRecord;
        public int DrilldownIndex { get; } = drilldownIndex;
        public bool IsCumulative { get; } = isCumulative;
        public bool IsProcessing { get; } = isProcessing;
        public string ReferenceCompoundName { get; } = referenceCompoundName;
        public double BodyWeight { get; } = bodyWeight;
    }
}
