namespace MCRA.Simulation.OutputGeneration {
    public class ChronicPerFoodAsEatenDrilldownSection(
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
        public List<IndividualFoodDrillDownRecord> IndividualFoodAsEatenDrillDownRecords { get; } = [];

        public void Summarize(IList<DietaryDayDrillDownRecord> drillDownRecords) {
            foreach (var dayDrillDown in drillDownRecords) {
                var records = dayDrillDown.IntakeSummaryPerFoodAsEatenRecords;
                foreach (var ipf in records) {
                    var individualFoodAsEatenDrilldownRecord = new IndividualFoodDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        FoodName = ipf.FoodName,
                        FoodCode = ipf.FoodCode,
                        TotalConsumption = ipf.GrossAmountConsumed,
                        NetConsumption = ipf.AmountConsumed,
                        EquivalentExposure = ipf.Concentration,
                        Exposure = ipf.IntakePerMassUnit,
                    };
                    IndividualFoodAsEatenDrillDownRecords.Add(individualFoodAsEatenDrilldownRecord);
                }
            }
        }
    }
}
