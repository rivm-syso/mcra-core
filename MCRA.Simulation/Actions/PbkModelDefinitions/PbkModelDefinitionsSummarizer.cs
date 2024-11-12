using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PbkModelDefinitions {
    public enum PbkModelDefinitionsSections {
        //No sub-sections yet
    }
    public sealed class PbkModelDefinitionsSummarizer : ActionResultsSummarizerBase<IPbkModelDefinitionsActionResult> {
        public override ActionType ActionType => ActionType.PbkModelDefinitions;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IPbkModelDefinitionsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<PbkModelDefinitionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PbkModelDefinitionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.PbkModelDefinitions);
            subHeader.SaveSummarySection(section);
        }
    }
}
