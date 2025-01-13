using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    public class HumanMonitoringDataSettingsSummarizer : ActionModuleSettingsSummarizer<HumanMonitoringDataModuleConfig> {

        public HumanMonitoringDataSettingsSummarizer(HumanMonitoringDataModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);

            section.SummarizeSetting(
                SettingsItemType.CodesHumanMonitoringSamplingMethods,
                string.Join(", ", _configuration.CodesHumanMonitoringSamplingMethods),
                _configuration.CodesHumanMonitoringSamplingMethods?.Count > 0
            );

            section.SummarizeSetting(SettingsItemType.MatchHbmIndividualSubsetWithPopulation, _configuration.MatchHbmIndividualSubsetWithPopulation);
            if (_configuration.MatchHbmIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedHbmSurveySubsetProperties, string.Join(", ", _configuration.SelectedHbmSurveySubsetProperties));
            }

            section.SummarizeSetting(SettingsItemType.ExcludeSubstancesFromSamplingMethod, _configuration.ExcludeSubstancesFromSamplingMethod);
            if (_configuration.ExcludeSubstancesFromSamplingMethod) {
                section.SummarizeSetting(SettingsItemType.ExcludedSubstancesFromSamplingMethodSubset, _configuration.ExcludedSubstancesFromSamplingMethodSubset.Count().ToString());
            }
            section.SummarizeSetting(SettingsItemType.UseCompleteAnalysedSamples, _configuration.UseCompleteAnalysedSamples);
            section.SummarizeSetting(SettingsItemType.UseHbmSamplingWeights, _configuration.UseHbmSamplingWeights);
            if (_configuration.FilterRepeatedMeasurements) {
                section.SummarizeSetting(SettingsItemType.FilterRepeatedMeasurements, _configuration.FilterRepeatedMeasurements);
                section.SummarizeSetting(SettingsItemType.RepeatedMeasurementTimepointCodes,
                    string.Join(", ", _configuration.RepeatedMeasurementTimepointCodes),
                    _configuration.RepeatedMeasurementTimepointCodes?.Count > 0);
            }
            return section;
        }
    }
}
