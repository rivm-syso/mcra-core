using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicIntakeEstimatesSection : SummarySection {
        public void SummarizeModel(SectionHeader header, ActionData data, BBNModel bbnModel) {
            var betaBinomialFrequencyModelSection = new BetaBinomialFrequencyModelSection();
            var subHeader = header?.AddSubSectionHeaderFor(betaBinomialFrequencyModelSection, "Beta binomial frequency model", 10);
            betaBinomialFrequencyModelSection.Summarize(bbnModel);
            subHeader?.SaveSummarySection(betaBinomialFrequencyModelSection);

            var normalAmountsModelSection = new NormalAmountsModelSection();
            subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelSection, "Normal amounts model", 40);
            normalAmountsModelSection.Summarize(bbnModel.AmountsModelSummary, bbnModel.IsAcuteCovariateModelling);
            subHeader?.SaveSummarySection(normalAmountsModelSection);

            var normalAmountsModelResidualSection = new NormalAmountsModelResidualSection();
            subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelResidualSection, "Normal amounts residual plots", 45);
            normalAmountsModelResidualSection.Summarize(bbnModel.AmountsModelSummary);
            subHeader?.SaveSummarySection(normalAmountsModelResidualSection);

            if (bbnModel.FrequencyModel.CovariateModel != CovariateModelType.Constant && bbnModel.FrequencyModel.CovariateModel != CovariateModelType.Cofactor) {
                var frequenciesModelGraphicsSection = new FrequenciesModelGraphicsSection();
                subHeader = header?.AddSubSectionHeaderFor(frequenciesModelGraphicsSection, "Frequency covariate plot", 30);
                frequenciesModelGraphicsSection.Summarize(data, bbnModel);
                subHeader?.SaveSummarySection(frequenciesModelGraphicsSection);
            }
            if (bbnModel.AmountModel.CovariateModel != CovariateModelType.Constant && bbnModel.AmountModel.CovariateModel != CovariateModelType.Cofactor) {
                var normalAmountsModelGraphicsSection = new NormalAmountsModelGraphicsSection();
                subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelGraphicsSection, "Normal amounts covariate plot", 50);
                normalAmountsModelGraphicsSection.Summarize(data, bbnModel);
                subHeader?.SaveSummarySection(normalAmountsModelGraphicsSection);
            }
        }

        public void SummarizeModel(SectionHeader header, ActionData data, LNN0Model lnn0Model) {
            var logisticFrequencyModelSection = new LogisticFrequencyModelSection();
            var subHeader = header?.AddSubSectionHeaderFor(logisticFrequencyModelSection, "Logistic normal frequency model", 20);
            logisticFrequencyModelSection.Summarize(lnn0Model);
            subHeader?.SaveSummarySection(logisticFrequencyModelSection);

            var normalAmountsModelSection = new NormalAmountsModelSection();
            subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelSection, "Normal amounts model", 40);
            normalAmountsModelSection.Summarize(lnn0Model.AmountsModelSummary, lnn0Model.IsAcuteCovariateModelling);
            subHeader?.SaveSummarySection(normalAmountsModelSection);

            var normalAmountsModelResidualSection = new NormalAmountsModelResidualSection();
            subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelResidualSection, "Normal amounts residual plots", 45);
            normalAmountsModelResidualSection.Summarize(lnn0Model.AmountsModelSummary);
            subHeader?.SaveSummarySection(normalAmountsModelResidualSection);

            if (lnn0Model.FrequencyModel.CovariateModel != CovariateModelType.Constant && lnn0Model.FrequencyModel.CovariateModel != CovariateModelType.Cofactor) {
                var frequenciesModelGraphicsSection = new FrequenciesModelGraphicsSection();
                subHeader = header?.AddSubSectionHeaderFor(frequenciesModelGraphicsSection, "Frequency covariate plot", 30);
                frequenciesModelGraphicsSection.Summarize(data, lnn0Model);
                subHeader?.SaveSummarySection(frequenciesModelGraphicsSection);
            }
            if (lnn0Model.AmountModel.CovariateModel != CovariateModelType.Constant && lnn0Model.AmountModel.CovariateModel != CovariateModelType.Cofactor) {
                var normalAmountsModelGraphicsSection = new NormalAmountsModelGraphicsSection();
                subHeader = header?.AddSubSectionHeaderFor(normalAmountsModelGraphicsSection, "Normal amounts covariate plot", 50);
                normalAmountsModelGraphicsSection.Summarize(data, lnn0Model);
                subHeader?.SaveSummarySection(normalAmountsModelGraphicsSection);
            }
        }
    }
}

