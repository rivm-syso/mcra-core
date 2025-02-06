namespace MCRA.Simulation.OutputGeneration {
    public class AcutePerFoodDrilldownSection(
        bool isModelledFood,
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
        public List<IndividualFoodDrillDownRecord> IndividualFoodDrillDownRecords { get; } = [];
        public bool IsModelledFood { get; } = isModelledFood;

        public void Summarize(
            IEnumerable<DietaryIntakeSummaryPerFoodRecord> dietaryIntakesPerModelledFood
        ) {
            foreach (var ipf in dietaryIntakesPerModelledFood) {
                var individualModellledFoodDrilldownRecord = new IndividualFoodDrillDownRecord() {
                    FoodName = ipf.FoodName,
                    FoodCode = ipf.FoodCode,
                    TotalConsumption = ipf.GrossAmountConsumed,
                    NetConsumption = ipf.AmountConsumed,
                    EquivalentExposure = ipf.Concentration,
                    Exposure = ipf.IntakePerMassUnit,
                };
                IndividualFoodDrillDownRecords.Add(individualModellledFoodDrilldownRecord);
            }
        }
    }
}
