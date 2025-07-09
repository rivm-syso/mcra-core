using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrationDistributions {

    public enum ConsumerProductConcentrationDistributionsSections {
        ConsumerProductConcentrationModelsSection,
        ConsumerProductConcentrationModelGraphsSection
    }

    public sealed class ConsumerProductConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<ConsumerProductConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProductConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, ConsumerProductConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(ConsumerProductConcentrationDistributionsSections.ConsumerProductConcentrationModelsSection)
                && data.ConsumerProductConcentrationModels?.Count > 0
            ) {
                summarizeConsumerProductConcentrationModels(
                    data.ConsumerProductConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(ConsumerProductConcentrationDistributionsSections.ConsumerProductConcentrationModelGraphsSection)
               && (data.ConsumerProductConcentrationModels?.Count > 0)) {
                summarizeConsumerProductConcentrationModelCharts(
                    data.ConsumerProductConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.ConsumerProductConcentrationUnit.GetShortDisplayName())
            };
            return result;
        }

        private void summarizeConsumerProductConcentrationModels(
           IDictionary<(ConsumerProduct, Compound), ConcentrationModel> concentrationModels,
           SectionHeader header,
           int order
       ) {
            var section = new ConsumerProductConcentrationModelsTableSection() {
                SectionLabel = getSectionLabel(ConsumerProductConcentrationDistributionsSections.ConsumerProductConcentrationModelsSection)
            };
            section.Summarize(concentrationModels);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer product concentration models per substance and consumer product",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeConsumerProductConcentrationModelCharts(
            IDictionary<(ConsumerProduct Product, Compound Substance), ConcentrationModel> concentrationModels,
            SectionHeader header,
            int subOrder
        ) {
            var section = new ConsumerProductConcentrationModelsGraphSection {
                SectionLabel = getSectionLabel(ConsumerProductConcentrationDistributionsSections.ConsumerProductConcentrationModelGraphsSection)
            };
            section.Summarize(concentrationModels);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer product concentration model graphs",
                subOrder++
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
