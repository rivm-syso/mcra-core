using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    public class HumanMonitoringDataSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringData;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var hms = project.HumanMonitoringSettings;
            section.SummarizeSetting(
                SettingsItemType.CodesHumanMonitoringSurveys, 
                string.Join(", ", hms.SurveyCodes),
                (hms.SurveyCodes?.Any() ?? false)
            );
            section.SummarizeSetting(
                SettingsItemType.CodesHumanMonitoringSamplingMethods,
                string.Join(", ", hms.SamplingMethodCodes),
                (hms.SamplingMethodCodes?.Any() ?? false)
            );

            section.SummarizeSetting(SettingsItemType.MatchHbmIndividualSubsetWithPopulation, project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation);
            if (project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedHbmSurveySubsetProperties, string.Join(", ", project.SelectedHbmSurveySubsetProperties));
            }
            section.SummarizeSetting(SettingsItemType.UseHbmSamplingWeights, project.SubsetSettings.UseHbmSamplingWeights);

            return section;
        }
    }
}
