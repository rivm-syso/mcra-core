using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class DustExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<DustExposuresModuleConfig> {
        public DustExposuresSettingsSummarizer(DustExposuresModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.DustExposures;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);

            section.SummarizeSetting(
                SettingsItemType.SelectedExposureRoutes,
                string.Join(", ", _configuration.SelectedExposureRoutes),
                _configuration.SelectedExposureRoutes.Count > 0
            );
            section.SummarizeSetting(SettingsItemType.DustExposuresIndividualGenerationMethod, _configuration.DustExposuresIndividualGenerationMethod);

            if (_configuration.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate) {
                section.SummarizeSetting(SettingsItemType.NumberOfSimulatedIndividuals, _configuration.NumberOfSimulatedIndividuals);
            }

            if (_configuration.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate &
                _configuration.ExposureType == ExposureType.Chronic) {
            }

            if (_configuration.SelectedExposureRoutes.Contains(ExposureRoute.Dermal)) {
                section.SummarizeSetting(SettingsItemType.DustTimeExposed, _configuration.DustTimeExposed);
            }

            return section;
        }
    }
}
