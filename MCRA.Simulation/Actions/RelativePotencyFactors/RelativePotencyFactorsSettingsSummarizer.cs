using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsSettingsSummarizer : ActionSettingsSummarizerBase {
        public override ActionType ActionType => ActionType.RelativePotencyFactors;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(project, section);
            var es = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.CodeReferenceCompound, es.CodeReferenceCompound, !string.IsNullOrEmpty(es.CodeReferenceCompound));
            return section;
        }
    }
}
