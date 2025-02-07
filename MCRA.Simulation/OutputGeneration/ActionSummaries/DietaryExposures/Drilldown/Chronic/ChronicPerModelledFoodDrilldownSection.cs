namespace MCRA.Simulation.OutputGeneration {
    public class ChronicPerModelledFoodDrilldownSection(
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
        public List<IndividualFoodDrillDownRecord> IndividualModelledFoodDrillDownRecords { get; } = [];

        public void Summarize(IList<DietaryDayDrillDownRecord> drillDownRecords) {
            foreach (var dayDrillDown in drillDownRecords) {
                var records = dayDrillDown.IntakeSummaryPerFoodAsMeasuredRecords;
                foreach (var record in records) {
                    var individualModellledFoodDrilldownRecord = new IndividualFoodDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        FoodName = record.FoodName,
                        FoodCode = record.FoodCode,
                        TotalConsumption = record.GrossAmountConsumed,
                        NetConsumption = record.AmountConsumed,
                        EquivalentExposure = record.Concentration,
                        Exposure = record.IntakePerMassUnit,
                    };
                    IndividualModelledFoodDrillDownRecords.Add(individualModellledFoodDrilldownRecord);
                }
            }
        }
    }
}
