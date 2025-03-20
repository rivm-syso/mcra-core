using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.AirExposures {

    public sealed class AirExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<AirExposuresModuleConfig> {
        public AirExposuresSettingsSummarizer(AirExposuresModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.AirExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(
                SettingsItemType.SelectedExposureRoutes,
                string.Join(", ", _configuration.SelectedExposureRoutes),
                _configuration.SelectedExposureRoutes.Count > 0
            );
            section.SummarizeSetting(SettingsItemType.AirExposuresIndividualGenerationMethod, _configuration.AirExposuresIndividualGenerationMethod);

            if (_configuration.AirExposuresIndividualGenerationMethod == AirExposuresIndividualGenerationMethod.Simulate) {
                section.SummarizeSetting(SettingsItemType.NumberOfSimulatedIndividuals, _configuration.NumberOfSimulatedIndividuals);
            }
            return section;
        }
    }
}
