using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ChronicIntakeInitialEstimatesSection : SummarySection {
        public void SummarizeModels(SectionHeader header, LNNModel lnnModel) {
            var lnnLogisticFrequencyModelSection = new LogisticFrequencyModelSection();
            var subHeader = header.AddSubSectionHeaderFor(lnnLogisticFrequencyModelSection, "Logistic normal frequency model", 20);
            lnnLogisticFrequencyModelSection.Summarize(lnnModel.FrequencyInitials);
            subHeader?.SaveSummarySection(lnnLogisticFrequencyModelSection);

            var lnnNormalAmountsModelSection = new NormalAmountsModelSection();
            subHeader = header.AddSubSectionHeaderFor(lnnNormalAmountsModelSection, "Normal amounts model", 30);
            lnnNormalAmountsModelSection.Summarize(lnnModel.AmountInitials);
            subHeader?.SaveSummarySection(lnnNormalAmountsModelSection);

            var lnnModelResultsSection = new LNNModelResultsSection();
            subHeader = header.AddSubSectionHeaderFor(lnnModelResultsSection, "LNN Model estimates (final)", 40);
            lnnModelResultsSection.Summarize(lnnModel);
            subHeader?.SaveSummarySection(lnnModelResultsSection);
        }
    }
}


