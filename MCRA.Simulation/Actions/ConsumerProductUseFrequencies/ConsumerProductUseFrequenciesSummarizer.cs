using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {
    public enum ConsumerProductUseFrequenciesSections {
        SurveysSection,
        IndividualStatisticsSection,
        ConsumerProductUseFrequenciesSection,
    }
    public sealed class ConsumerProductUseFrequenciesSummarizer : ActionResultsSummarizerBase<IConsumerProductUseFrequenciesActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProductUseFrequencies;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductUseFrequenciesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductUseFrequenciesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new CPDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            if (outputSettings.ShouldSummarize(ConsumerProductUseFrequenciesSections.SurveysSection)) {
                summarizeSurveys(
                    data.ConsumerProductSurveys.First(),
                    data.SelectedPopulation,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(ConsumerProductUseFrequenciesSections.IndividualStatisticsSection)) {
                summarizeIndividualStatistics(
                    data.ConsumerProductIndividuals,
                    data.SelectedPopulation,
                    subHeader,
                    subOrder++
                );
            }

        }
        private void summarizeSurveys(
            ConsumerProductSurvey survey,
            Population population,
            SectionHeader header,
            int order
        ) {
            var section = new CPSurveySummarySection() {
                SectionLabel = getSectionLabel(ConsumerProductUseFrequenciesSections.SurveysSection)
            };
            section.Summarize(
                survey,
                population
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer product study",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeIndividualStatistics(
            ICollection<Individual> individuals,
            Population population,
            SectionHeader header,
            int order
        ) {
            var section = new CPIndividualStatisticsSummarySection() {
                SectionLabel = getSectionLabel(ConsumerProductUseFrequenciesSections.IndividualStatisticsSection)
            };
            section.Summarize(
                individuals,
                population,
                //_configuration.MatchHbmIndividualSubsetWithPopulation,
                //_configuration.SelectedHbmSurveySubsetProperties,
                //_configuration.SkipPrivacySensitiveOutputs
                IndividualSubsetType.IgnorePopulationDefinition, 
                [], 
                false
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Individuals",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
