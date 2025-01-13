using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Effects {

    public class EffectsSettingsSummarizer : ActionModuleSettingsSummarizer<EffectsModuleConfig> {

        public EffectsSettingsSummarizer(EffectsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.MultipleEffects, _configuration.MultipleEffects);
            if (!_configuration.MultipleEffects) {
                section.SummarizeSetting(SettingsItemType.IncludeAopNetwork, _configuration.IncludeAopNetwork);
                if (_configuration.IncludeAopNetwork) {
                    section.SummarizeSetting(SettingsItemType.CodeFocalEffect, _configuration.CodeFocalEffect, !string.IsNullOrEmpty(_configuration.CodeFocalEffect));
                }
            }
            return section;
        }
    }
}
