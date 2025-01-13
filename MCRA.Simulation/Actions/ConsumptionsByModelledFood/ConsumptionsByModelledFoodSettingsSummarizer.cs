using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {

    public sealed class ConsumptionsByModelledFoodSettingsSummarizer : ActionModuleSettingsSummarizer<ConsumptionsByModelledFoodModuleConfig> {
        public ConsumptionsByModelledFoodSettingsSummarizer(ConsumptionsByModelledFoodModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ModelledFoodsConsumerDaysOnly, _configuration.ModelledFoodsConsumerDaysOnly);
            if (_configuration.ModelledFoodsConsumerDaysOnly) {
                if (project.ActionType == ActionType) {
                    section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
                }
                section.SummarizeSetting(SettingsItemType.RestrictPopulationByModelledFoodSubset, _configuration.RestrictPopulationByModelledFoodSubset);
                if (_configuration.RestrictPopulationByModelledFoodSubset) {
                    section.SummarizeSetting(SettingsItemType.FocalFoodAsMeasuredSubset, string.Join(",", _configuration.FocalFoodAsMeasuredSubset));
                }
            }
            return section;
        }
    }
}
