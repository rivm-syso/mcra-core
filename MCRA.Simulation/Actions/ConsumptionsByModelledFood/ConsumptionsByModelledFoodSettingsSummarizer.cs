using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {

    public sealed class ConsumptionsByModelledFoodSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ConsumptionsByModelledFood;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ModelledFoodsConsumerDaysOnly, project.SubsetSettings.ModelledFoodsConsumerDaysOnly);
            if (project.SubsetSettings.ModelledFoodsConsumerDaysOnly) {
                if (project.ActionType == ActionType) {
                    section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
                }
                section.SummarizeSetting(SettingsItemType.RestrictPopulationByModelledFoodSubset, project.SubsetSettings.RestrictPopulationByModelledFoodSubset);
                if (project.SubsetSettings.RestrictPopulationByModelledFoodSubset) {
                    section.SummarizeSetting(SettingsItemType.FocalFoodAsMeasuredSubset, string.Join(",", project.FocalFoodAsMeasuredSubset));
                }
            }
            return section;
        }
    }
}
