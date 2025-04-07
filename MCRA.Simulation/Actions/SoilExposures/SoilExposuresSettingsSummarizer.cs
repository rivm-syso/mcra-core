using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class SoilExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<SoilExposuresModuleConfig> {
        public SoilExposuresSettingsSummarizer(SoilExposuresModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.SoilExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.SoilExposuresIndividualGenerationMethod, _configuration.SoilExposuresIndividualGenerationMethod);

            return section;
        }
    }
}
