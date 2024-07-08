using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ModelledFoods {

    public sealed class ModelledFoodsSettingsSummarizer : ActionModuleSettingsSummarizer<ModelledFoodsModuleConfig> {

        public ModelledFoodsSettingsSummarizer(ModelledFoodsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.DeriveModelledFoodsFromSampleBasedConcentrations, _configuration.DeriveModelledFoodsFromSampleBasedConcentrations);
            section.SummarizeSetting(SettingsItemType.DeriveModelledFoodsFromSingleValueConcentrations, _configuration.DeriveModelledFoodsFromSingleValueConcentrations);
            section.SummarizeSetting(SettingsItemType.UseWorstCaseValues, _configuration.UseWorstCaseValues);
            if (_configuration.RestrictToModelledFoodSubset) {
                section.SummarizeSetting(SettingsItemType.RestrictToModelledFoodSubset, _configuration.RestrictToModelledFoodSubset);
                section.SummarizeSetting(SettingsItemType.ModelledFoodSubset, string.Join(",", _configuration.ModelledFoodSubset));
            }
            if (_configuration.FoodIncludeNonDetects) {
                section.SummarizeSetting(SettingsItemType.FoodIncludeNonDetects, _configuration.FoodIncludeNonDetects);
            }
            if (_configuration.SubstanceIncludeNonDetects) {
                section.SummarizeSetting(SettingsItemType.SubstanceIncludeNonDetects, _configuration.SubstanceIncludeNonDetects);
            }
            return section;
        }
    }
}
