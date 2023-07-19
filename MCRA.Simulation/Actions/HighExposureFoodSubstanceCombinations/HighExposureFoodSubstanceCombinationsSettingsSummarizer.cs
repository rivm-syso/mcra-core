using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {

    public class HighExposureFoodSubstanceCombinationsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HighExposureFoodSubstanceCombinations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var settings = project.ScreeningSettings;
            section.SummarizeSetting(SettingsItemType.CriticalExposurePercentage, settings.CriticalExposurePercentage);
            section.SummarizeSetting(SettingsItemType.CumulativeSelectionPercentage, settings.CumulativeSelectionPercentage);
            section.SummarizeSetting(SettingsItemType.ImportanceLor, settings.ImportanceLor);
            return section;
        }
    }
}
