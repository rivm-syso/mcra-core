using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Populations {

    public class PopulationsSettingsSummarizer : ActionModuleSettingsSummarizer<PopulationsModuleConfig> {

        public PopulationsSettingsSummarizer(PopulationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary("Populations");
            if (project.PopulationsSettings.IsCompute) {
                section.SummarizeSetting(SettingsItemType.NominalPopulationBodyWeight, _configuration.NominalPopulationBodyWeight);
                if (_configuration.PopulationSubsetSelection) {
                    section.SummarizeSetting(SettingsItemType.PopulationSubsetSelection, _configuration.PopulationSubsetSelection);
                    foreach (var subset in _configuration.IndividualsSubsetDefinitions) {
                        section.SummarizeSetting(subset.NameIndividualProperty, subset.IndividualPropertyQuery);
                    }
                    var isMonthsSubset = _configuration.IndividualDaySubsetDefinition?.MonthsSubset?.Count > 0;
                    section.SummarizeSetting(SettingsItemType.FilterIndividualDaysByMonth, isMonthsSubset);
                    if (isMonthsSubset) {
                        section.SummarizeSetting(SettingsItemType.IndividualDayMonths, string.Join(", ", _configuration.IndividualDaySubsetDefinition.MonthsSubset));
                    }
                }
            } else {
                summarizeDataSources(project, section);
            }
            //section.SummarizeSetting(SettingsItemType.CodePopulation, ps.CodePopulation, !string.IsNullOrEmpty(ps.CodePopulation));
            return section;
        }
    }
}
