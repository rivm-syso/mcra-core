namespace MCRA.Simulation.OutputGeneration {
    public abstract class AcuteDrilldownSectionBase(
        int drilldownIndex,
        OverallIndividualDayDrillDownRecord drilldownRecord,
        bool isCumulative,
        bool isProcessing,
        bool isUnitVariability,
        string referenceCompoundName
    ) : SummarySection {
        public int DrilldownIndex { get; } = drilldownIndex;
        public OverallIndividualDayDrillDownRecord DrilldownRecord { get; } = drilldownRecord;
        public bool IsCumulative { get; } = isCumulative;
        public bool IsProcessing { get; } = isProcessing;
        public bool IsUnitVariability { get; } = isUnitVariability;
        public string ReferenceCompoundName { get; } = referenceCompoundName;
    }
}
