namespace MCRA.Simulation.OutputGeneration {
    public class ChronicPerSubstanceDrilldownSection(
        int drilldownIndex,
        OverallIndividualDrillDownRecord drilldownRecord,
        bool isCumulative,
        bool isProcessing,
        string referenceCompoundName,
        double bodyWeight
    ) : ChronicDrilldownSectionBase(
        drilldownRecord,
        drilldownIndex,
        isCumulative,
        isProcessing,
        referenceCompoundName,
        bodyWeight
    ) {
        public List<IndividualSubstanceDrillDownRecord> IndividualSubstanceDrillDownRecords { get; } = [];

        public void Summarize(
            IList<DietaryDayDrillDownRecord> drillDownRecords
        ) {
            foreach (var dayDrillDown in drillDownRecords) {
                var records = dayDrillDown.DietaryIntakeSummaryPerCompoundRecords;
                foreach (var record in records) {
                    var individualSubstanceDrilldownRecord = new IndividualSubstanceDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        SubstanceName = record.CompoundName,
                        SubstanceCode = record.CompoundCode,
                        ExposurePerDay = BodyWeight * record.DietaryIntakeAmountPerBodyWeight / record.RelativePotencyFactor,
                        Exposure = record.DietaryIntakeAmountPerBodyWeight / record.RelativePotencyFactor,
                        Rpf = record.RelativePotencyFactor,
                        EquivalentExposure = record.DietaryIntakeAmountPerBodyWeight
                    };
                    IndividualSubstanceDrillDownRecords.Add(individualSubstanceDrilldownRecord);
                }
            }
        }
    }
}
