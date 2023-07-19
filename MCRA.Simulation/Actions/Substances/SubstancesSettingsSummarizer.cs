using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Substances {

    public class SubstancesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Substances;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            if (project.AssessmentSettings.MultipleSubstances) {
                section.SummarizeSetting(SettingsItemType.MultipleSubstances, project.AssessmentSettings.MultipleSubstances);
            }
            return section;
        }
    }
}
