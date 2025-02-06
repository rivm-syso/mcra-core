namespace MCRA.Simulation.OutputGeneration {
    public class AcuteIndividualDayDrilldownSection : SummarySection {
        public void Summarize(
            SectionHeader header,
            int drilldownIndex,
            OverallIndividualDayDrillDownRecord drilldownRecord,
            IEnumerable<DietaryAcuteIntakePerFoodRecord> dietaryAcuteFoodRecords,
            IEnumerable<DietaryIntakeSummaryPerCompoundRecord> dietaryIntakesPerCompound,
            IEnumerable<DietaryIntakeSummaryPerFoodRecord> dietaryIntakesPerModelledFood,
            IEnumerable<DietaryIntakeSummaryPerFoodRecord> dietaryIntakesPerFoodAsEaten,
            bool isCumulative,
            bool isProcessing,
            bool isUnitVariability,
            string referenceCompoundName,
            double dietaryIntakePerBodyWeight
        ) {
            var detailedIndividualDrilldownSection = new AcutePerPortionPerSubstanceDrilldownSection(
                drilldownRecord,
                drilldownIndex,
                isCumulative,
                isProcessing,
                isUnitVariability,
                referenceCompoundName
            );
            var subHeader = header.AddSubSectionHeaderFor(detailedIndividualDrilldownSection, "Per consumed portion per substance", 1);
            detailedIndividualDrilldownSection.Summarize(
                dietaryAcuteFoodRecords,
                dietaryIntakePerBodyWeight
            );
            subHeader.SaveSummarySection(detailedIndividualDrilldownSection);

            var perSubstanceSection = new AcutePerSubstanceDrilldownSection(
                drilldownRecord,
                drilldownIndex,
                isCumulative,
                isProcessing,
                isUnitVariability,
                referenceCompoundName
            );
            subHeader = header.AddSubSectionHeaderFor(perSubstanceSection, "Per substance", 2);
            perSubstanceSection.Summarize(dietaryIntakesPerCompound);
            subHeader.SaveSummarySection(perSubstanceSection);

            var perModelledFoodSection = new AcutePerFoodDrilldownSection(
                true,
                drilldownRecord,
                drilldownIndex,
                isCumulative,
                isProcessing,
                isUnitVariability,
                referenceCompoundName
            );
            subHeader = header.AddSubSectionHeaderFor(perModelledFoodSection, "Per modelled food", 3);
            perModelledFoodSection.Summarize(dietaryIntakesPerModelledFood);
            subHeader.SaveSummarySection(perModelledFoodSection);

            var perFoodAsEatenSection = new AcutePerFoodDrilldownSection(
                false,
                drilldownRecord,
                drilldownIndex,
                isCumulative,
                isProcessing,
                isUnitVariability,
                referenceCompoundName
            );
            subHeader = header.AddSubSectionHeaderFor(perFoodAsEatenSection, "Per food as eaten", 4);
            perFoodAsEatenSection.Summarize(dietaryIntakesPerFoodAsEaten);
            subHeader.SaveSummarySection(perFoodAsEatenSection);
        }
    }
}
