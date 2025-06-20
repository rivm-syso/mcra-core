using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {

    public class ConsumerProductUseFrequenciesSettingsSummarizer(ConsumerProductUseFrequenciesModuleConfig config) : ActionModuleSettingsSummarizer<ConsumerProductUseFrequenciesModuleConfig>(config) {

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            section.SummarizeSetting(SettingsItemType.MatchCPIndividualSubsetWithPopulation, _configuration.MatchCPIndividualSubsetWithPopulation);
            if (_configuration.MatchCPIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                section.SummarizeSetting(SettingsItemType.SelectedCPSurveySubsetProperties, string.Join(", ", _configuration.SelectedCPSurveySubsetProperties));
            }
            section.SummarizeSetting(SettingsItemType.UseCPSamplingWeights, _configuration.UseCPSamplingWeights);
            return section;
        }
    }
}
