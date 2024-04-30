using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {

    public sealed class FocalFoodConcentrationsSettingsSummarizer : ActionModuleSettingsSummarizer<FocalFoodConcentrationsModuleConfig> {

        public FocalFoodConcentrationsSettingsSummarizer(FocalFoodConcentrationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeFood)));
            section.SummarizeSetting(SettingsItemType.FocalSubstances, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeSubstance)));
            return section;
        }
    }
}
