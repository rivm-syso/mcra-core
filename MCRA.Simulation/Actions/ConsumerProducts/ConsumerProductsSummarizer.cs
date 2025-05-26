using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ConsumerProducts {
    public enum ConsumerProductsSections {
        ConsumerProductsSection,
    }
    public sealed class ConsumerProductsSummarizer : ActionResultsSummarizerBase<IConsumerProductsActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProducts;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize consumer products
            if (outputSettings.ShouldSummarize(ConsumerProductsSections.ConsumerProductsSection)) {
                summarizeConsumerProducts(data, subHeader, subOrder++);
            }
        }

        private  void summarizeConsumerProducts(ActionData data, SectionHeader header, int order) {
            var section = new ConsumerProductsSummarySection {
                SectionLabel = getSectionLabel(ConsumerProductsSections.ConsumerProductsSection)
            };

            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer products",
                order
            );

            section.Summarize(data.AllConsumerProducts);
            subHeader.SaveSummarySection(section);
        }
    }
}
