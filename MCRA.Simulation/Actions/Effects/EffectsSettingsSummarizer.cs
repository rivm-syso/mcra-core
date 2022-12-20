using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Effects {

    public class EffectsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Effects;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var es = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.MultipleEffects, es.MultipleEffects);
            if (!es.MultipleEffects) {
                section.SummarizeSetting(SettingsItemType.IncludeAopNetwork, es.IncludeAopNetwork);
                if (es.IncludeAopNetwork) {
                    section.SummarizeSetting(SettingsItemType.CodeFocalEffect, es.CodeFocalEffect, !string.IsNullOrEmpty(es.CodeFocalEffect));
                }
            }
            return section;
        }
    }
}
