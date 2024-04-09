using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {

    public class SingleValueNonDietaryExposuresSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.SingleValueNonDietaryExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            section.SummarizeSetting(SettingsItemType.CodeConfiguration, project.NonDietarySettings.CodeConfiguration);
            return section;
        }
    }
}
