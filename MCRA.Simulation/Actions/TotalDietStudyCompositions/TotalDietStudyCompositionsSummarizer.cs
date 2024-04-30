using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.TotalDietStudyCompositions {
    public enum TotalDietStudyCompositionsSections {
        //No sub-sections
    }
    public sealed class TotalDietStudyCompositionsSummarizer : ActionResultsSummarizerBase<ITotalDietStudyCompositionsActionResult> {
        public override ActionType ActionType => ActionType.TotalDietStudyCompositions;

        public override void Summarize(ActionModuleConfig sectionConfig, ITotalDietStudyCompositionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<TotalDietStudyCompositionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new TotalDietStudyCompositionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(subHeader, data.TdsFoodCompositions);
            subHeader.SaveSummarySection(section);
        }
    }
}
