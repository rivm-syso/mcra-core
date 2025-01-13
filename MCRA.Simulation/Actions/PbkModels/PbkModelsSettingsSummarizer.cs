using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.PbkModels {

    public class PbkModelsSettingsSummarizer : ActionModuleSettingsSummarizer<PbkModelsModuleConfig> {

        public PbkModelsSettingsSummarizer(PbkModelsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
            section.SummarizeSetting(SettingsItemType.NumberOfDays, _configuration.NumberOfDays);
            section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
            section.SummarizeSetting(SettingsItemType.SpecifyEvents, _configuration.SpecifyEvents);
            if (_configuration.SpecifyEvents) {
                section.SummarizeSetting(SettingsItemType.SelectedEvents, _configuration.SelectedEvents);
            }
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Oral)) {
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryOral, _configuration.NumberOfDosesPerDayNonDietaryOral);
            }
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Dermal)) {
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryDermal, _configuration.NumberOfDosesPerDayNonDietaryDermal);
            }
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Inhalation)) {
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryInhalation, _configuration.NumberOfDosesPerDayNonDietaryInhalation);
            }
            return section;
        }
    }
}
