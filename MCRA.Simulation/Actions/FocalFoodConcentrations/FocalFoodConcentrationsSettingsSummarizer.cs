using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {

    public sealed class FocalFoodConcentrationsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.FocalFoodConcentrations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var cms = project.ConcentrationModelSettings;
            section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", project.FocalFoods.Select(r => r.CodeFood)));
            section.SummarizeSetting(SettingsItemType.FocalSubstances, string.Join(", ", project.FocalFoods.Select(r => r.CodeSubstance)));
            return section;
        }
    }
}
