using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    public class HumanMonitoringDataSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringData;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);

            var hms = project.HumanMonitoringSettings;
            section.SummarizeSetting(
                SettingsItemType.CodesHumanMonitoringSamplingMethods,
                string.Join(", ", hms.SamplingMethodCodes),
                (hms.SamplingMethodCodes?.Any() ?? false)
            );

            section.SummarizeSetting(SettingsItemType.MatchHbmIndividualSubsetWithPopulation, project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation);
            if (project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedHbmSurveySubsetProperties, string.Join(", ", project.SelectedHbmSurveySubsetProperties));
            }
            
            section.SummarizeSetting(SettingsItemType.ExcludeSubstancesFromSamplingMethod, project.HumanMonitoringSettings.ExcludeSubstancesFromSamplingMethod);
            if (project.HumanMonitoringSettings.ExcludeSubstancesFromSamplingMethod) {
                section.SummarizeSetting(SettingsItemType.ExcludedSubstancesFromSamplingMethodSubset, project.HumanMonitoringSettings.ExcludedSubstancesFromSamplingMethodSubset.Count().ToString());
            }
            section.SummarizeSetting(SettingsItemType.UseCompleteAnalysedSamples, project.HumanMonitoringSettings.UseCompleteAnalysedSamples);
            section.SummarizeSetting(SettingsItemType.UseHbmSamplingWeights, project.SubsetSettings.UseHbmSamplingWeights);
            return section;
        }
    }
}
