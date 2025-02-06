namespace MCRA.Simulation.OutputGeneration {
    public class AcutePerSubstanceDrilldownSection(
        OverallIndividualDayDrillDownRecord drilldownRecord,
        int drilldownIndex,
        bool isCumulative,
        bool isProcessing,
        bool isUnitVariability,
        string referenceCompoundName
    ) : AcuteDrilldownSectionBase(
        drilldownIndex,
        drilldownRecord,
        isCumulative,
        isProcessing,
        isUnitVariability,
        referenceCompoundName
    ) {
        public List<IndividualSubstanceDrillDownRecord> IndividualSubstanceDrillDownRecords { get; } = [];

        public void Summarize(
            IEnumerable<DietaryIntakeSummaryPerCompoundRecord> dietaryIntakesPerCompound
        ) {
            foreach (var ipc in dietaryIntakesPerCompound) {
                var individualSubstanceDrilldownRecord = new IndividualSubstanceDrillDownRecord() {
                    SubstanceName = ipc.CompoundName,
                    SubstanceCode = ipc.CompoundCode,
                    ExposurePerDay = DrilldownRecord.BodyWeight * ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor,
                    Exposure = ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor,
                    Rpf = ipc.RelativePotencyFactor,
                    EquivalentExposure = ipc.DietaryIntakeAmountPerBodyWeight
                };
                IndividualSubstanceDrillDownRecords.Add(individualSubstanceDrilldownRecord);
            }
        }
    }
}
