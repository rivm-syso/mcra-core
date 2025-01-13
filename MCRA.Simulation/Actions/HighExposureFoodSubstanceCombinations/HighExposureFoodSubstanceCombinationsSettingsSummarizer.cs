using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {

    public class HighExposureFoodSubstanceCombinationsSettingsSummarizer : ActionModuleSettingsSummarizer<HighExposureFoodSubstanceCombinationsModuleConfig> {

        public HighExposureFoodSubstanceCombinationsSettingsSummarizer(HighExposureFoodSubstanceCombinationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.CriticalExposurePercentage, _configuration.CriticalExposurePercentage);
            section.SummarizeSetting(SettingsItemType.CumulativeSelectionPercentage, _configuration.CumulativeSelectionPercentage);
            section.SummarizeSetting(SettingsItemType.ImportanceLor, _configuration.ImportanceLor);
            return section;
        }
    }
}
