namespace MCRA.Simulation.OutputGeneration {
    public class ChronicIndividualDrilldownSection: SummarySection {
        public void Summarize(
            SectionHeader header,
            int drilldownIndex,
            OverallIndividualDrillDownRecord drilldownRecord,
            IList<DietaryDayDrillDownRecord> dayDrilldownRecords,
            double bodyWeight,
            double individualMean,
            bool isCumulative,
            bool isProcessing,
            string referenceCompoundName
        ) {

            var detailedDrilldownSection = new ChronicDetailedIndividualDrilldownSection(
                drilldownIndex,
                drilldownRecord,
                isCumulative,
                isProcessing,
                referenceCompoundName,
                bodyWeight,
                individualMean
            );
            var subHeader = header.AddSubSectionHeaderFor(detailedDrilldownSection, "Per consumed portion per substance", 1);
            detailedDrilldownSection.Summarize(dayDrilldownRecords);
            subHeader.SaveSummarySection(detailedDrilldownSection);

            var perSubstanceSection = new ChronicPerSubstanceDrilldownSection(
                drilldownIndex,
                drilldownRecord,
                isCumulative,
                isProcessing,
                referenceCompoundName,
                bodyWeight
            );
            subHeader = header.AddSubSectionHeaderFor(perSubstanceSection, "Per substance", 2);
            perSubstanceSection.Summarize(dayDrilldownRecords);
            subHeader.SaveSummarySection(perSubstanceSection);

            var perModelledFoodSection = new ChronicPerModelledFoodDrilldownSection(
                drilldownIndex,
                drilldownRecord,
                isCumulative,
                isProcessing,
                referenceCompoundName,
                bodyWeight
            );
            subHeader = header.AddSubSectionHeaderFor(perModelledFoodSection, "Per modelled food", 3);
            perModelledFoodSection.Summarize(dayDrilldownRecords);
            subHeader.SaveSummarySection(perModelledFoodSection);

            var perFoodAsEatenSection = new ChronicPerFoodAsEatenDrilldownSection(
                drilldownIndex,
                drilldownRecord,
                isCumulative,
                isProcessing,
                referenceCompoundName,
                bodyWeight
            );
            subHeader = header.AddSubSectionHeaderFor(perFoodAsEatenSection, "Per food as eaten", 4);
            perFoodAsEatenSection.Summarize(dayDrilldownRecords);
            subHeader.SaveSummarySection(perFoodAsEatenSection);
        }
    }
}
