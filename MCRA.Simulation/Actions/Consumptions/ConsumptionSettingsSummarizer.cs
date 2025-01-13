using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Consumptions {

    public sealed class ConsumptionSettingsSummarizer : ActionModuleSettingsSummarizer<ConsumptionsModuleConfig> {

        public ConsumptionSettingsSummarizer(ConsumptionsModuleConfig config): base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);

            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.ConsumerDaysOnly, _configuration.ConsumerDaysOnly);
            if (_configuration.ConsumerDaysOnly) {
                section.SummarizeSetting(SettingsItemType.RestrictPopulationByFoodAsEatenSubset, _configuration.RestrictPopulationByFoodAsEatenSubset);
                if (_configuration.RestrictPopulationByFoodAsEatenSubset) {
                    section.SummarizeSetting(SettingsItemType.FocalFoodAsEatenSubset, string.Join(",", _configuration.FocalFoodAsEatenSubset));
                }
            }
            section.SummarizeSetting(SettingsItemType.RestrictConsumptionsByFoodAsEatenSubset, _configuration.RestrictConsumptionsByFoodAsEatenSubset);
            if (_configuration.RestrictConsumptionsByFoodAsEatenSubset) {
                section.SummarizeSetting(SettingsItemType.FoodAsEatenSubset, string.Join(",", _configuration.FoodAsEatenSubset));
            }
            section.SummarizeSetting(SettingsItemType.IsDefaultSamplingWeight, _configuration.IsDefaultSamplingWeight);
            if (_configuration.NameCofactor != string.Empty) {
                section.SummarizeSetting("Cofactor name", _configuration.NameCofactor);
            }
            if (_configuration.NameCovariable != string.Empty) {
                section.SummarizeSetting("Covariable name", _configuration.NameCovariable);
            }

            section.SummarizeSetting(SettingsItemType.MatchIndividualSubsetWithPopulation, _configuration.MatchIndividualSubsetWithPopulation);
            if (_configuration.MatchIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedFoodSurveySubsetProperties, string.Join(", ", _configuration.SelectedFoodSurveySubsetProperties));
            }

            if (_configuration.ExposureType == ExposureType.Chronic) {
                section.SummarizeSetting(SettingsItemType.ExcludeIndividualsWithLessThanNDays, _configuration.ExcludeIndividualsWithLessThanNDays);
                if (_configuration.ExcludeIndividualsWithLessThanNDays) {
                    section.SummarizeSetting(SettingsItemType.MinimumNumberOfDays, _configuration.MinimumNumberOfDays);
                }
            }
            return section;
        }
    }
}
