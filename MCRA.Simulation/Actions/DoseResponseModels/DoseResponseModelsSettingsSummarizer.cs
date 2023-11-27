using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DoseResponseModels {
    public sealed class DoseResponseModelsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.DoseResponseModels;
        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.CalculateParametricConfidenceInterval, project.DoseResponseModelsSettings.CalculateParametricConfidenceInterval);
            return section;
        }
    }
}
