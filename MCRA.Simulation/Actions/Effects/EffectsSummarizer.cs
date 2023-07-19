using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Effects {
    public enum EffectsSection {
        //No sub-sections
    }
    public sealed class EffectsSummarizer : ActionResultsSummarizerBase<IEffectsActionResult> {

        public override ActionType ActionType => ActionType.Effects;

        public override void Summarize(ProjectDto project, IEffectsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<EffectsSection>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new EffectsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.Effects.GetDisplayName(), order);
            section.Summarize(
                data.AllEffects,
                data.SelectedEffect?.Code
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
