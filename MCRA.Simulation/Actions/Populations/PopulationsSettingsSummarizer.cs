using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Populations {

    public class PopulationsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Populations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary("Populations");
            var ps = project.PopulationSettings;
            if (project.CalculationActionTypes.Contains(ActionType.Populations)) {
                section.SummarizeSetting(SettingsItemType.NominalPopulationBodyWeight, ps.NominalPopulationBodyWeight);
                if (project.SubsetSettings.PopulationSubsetSelection) {
                    section.SummarizeSetting(SettingsItemType.PopulationSubsetSelection, project.SubsetSettings.PopulationSubsetSelection);
                    foreach (var subset in project.IndividualsSubsetDefinitions) {
                        section.SummarizeSetting(subset.NameIndividualProperty, subset.IndividualPropertyQuery);
                    }
                    var isMonthsSubset = project.IndividualDaySubsetDefinition?.MonthsSubset?.Any() ?? false;
                    section.SummarizeSetting(SettingsItemType.FilterIndividualDaysByMonth, isMonthsSubset);
                    if (isMonthsSubset) {
                        section.SummarizeSetting(SettingsItemType.IndividualDayMonths, string.Join(", ", project.IndividualDaySubsetDefinition.MonthsSubset));
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
