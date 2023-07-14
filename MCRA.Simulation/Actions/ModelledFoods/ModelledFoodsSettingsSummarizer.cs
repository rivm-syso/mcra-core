using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ModelledFoods {

    public sealed class ModelledFoodsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ModelledFoods;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var cs = project.ConversionSettings;
            section.SummarizeSetting(SettingsItemType.DeriveModelledFoodsFromSampleBasedConcentrations, cs.DeriveModelledFoodsFromSampleBasedConcentrations);
            section.SummarizeSetting(SettingsItemType.DeriveModelledFoodsFromSingleValueConcentrations, cs.DeriveModelledFoodsFromSingleValueConcentrations);
            section.SummarizeSetting(SettingsItemType.UseWorstCaseValues, cs.UseWorstCaseValues);
            if (project.SubsetSettings.RestrictToModelledFoodSubset) {
                section.SummarizeSetting(SettingsItemType.RestrictToModelledFoodSubset, project.SubsetSettings.RestrictToModelledFoodSubset);
                section.SummarizeSetting(SettingsItemType.ModelledFoodSubset, string.Join(",", project.ModelledFoodSubset));
            }
            if (cs.FoodIncludeNonDetects) {
                section.SummarizeSetting(SettingsItemType.FoodIncludeNonDetects, cs.FoodIncludeNonDetects);
            }
            if (cs.CompoundIncludeNonDetects) {
                section.SummarizeSetting(SettingsItemType.CompoundIncludeNonDetects, cs.CompoundIncludeNonDetects);
            }
            return section;
        }
    }
}
