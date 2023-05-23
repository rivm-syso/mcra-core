using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Consumptions {

    public sealed class ConsumptionSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Consumptions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var isCompute = project.CalculationActionTypes.Contains(ActionType.Populations);
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            section.SummarizeSetting(SettingsItemType.ConsumptionsTier, project.FoodSurveySettings.ConsumptionsTier);
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.ConsumerDaysOnly, project.SubsetSettings.ConsumerDaysOnly);
            if (project.SubsetSettings.ConsumerDaysOnly) {
                section.SummarizeSetting(SettingsItemType.RestrictPopulationByFoodAsEatenSubset, project.SubsetSettings.RestrictPopulationByFoodAsEatenSubset);
                if (project.SubsetSettings.RestrictPopulationByFoodAsEatenSubset) {
                    section.SummarizeSetting(SettingsItemType.FocalFoodAsEatenSubset, string.Join(",", project.FocalFoodAsEatenSubset));
                }
            }
            section.SummarizeSetting(SettingsItemType.RestrictConsumptionsByFoodAsEatenSubset, project.SubsetSettings.RestrictConsumptionsByFoodAsEatenSubset);
            if (project.SubsetSettings.RestrictConsumptionsByFoodAsEatenSubset) {
                section.SummarizeSetting(SettingsItemType.FoodAsEatenSubset, string.Join(",", project.FoodAsEatenSubset.Select(r => r.CodeFood)));
            }
            section.SummarizeSetting(SettingsItemType.IsDefaultSamplingWeight, project.SubsetSettings.IsDefaultSamplingWeight);
            if (project.CovariatesSelectionSettings.NameCofactor != string.Empty) {
                section.SummarizeSetting("Cofactor name", project.CovariatesSelectionSettings.NameCofactor);
            }
            if (project.CovariatesSelectionSettings.NameCovariable != string.Empty) {
                section.SummarizeSetting("Covariable name", project.CovariatesSelectionSettings.NameCovariable);
            }

            section.SummarizeSetting(SettingsItemType.MatchIndividualSubsetWithPopulation, project.SubsetSettings.MatchIndividualSubsetWithPopulation);
            if (project.SubsetSettings.MatchIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedFoodSurveySubsetProperties, string.Join(", ", project.SelectedFoodSurveySubsetProperties));
            }

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                section.SummarizeSetting(SettingsItemType.ExcludeIndividualsWithLessThanNDays, project.SubsetSettings.ExcludeIndividualsWithLessThanNDays);
                if (project.SubsetSettings.ExcludeIndividualsWithLessThanNDays) {
                    section.SummarizeSetting(SettingsItemType.MinimumNumberOfDays, project.SubsetSettings.MinimumNumberOfDays);
                }
            }
            return section;
        }
    }
}
