using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {
    public enum ConsumerProductUseFrequenciesSections {
        ConsumerProductUseFrequenciesSection,
    }
    public sealed class ConsumerProductUseFrequenciesSummarizer : ActionResultsSummarizerBase<IConsumerProductUseFrequenciesActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProductUseFrequencies;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductUseFrequenciesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductUseFrequenciesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

        }
    }
}
