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

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var isAggregate = base._configuration.Aggregate;
            section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
            section.SummarizeSetting(SettingsItemType.NumberOfDays, _configuration.NumberOfDays);
            section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
            section.SummarizeSetting(SettingsItemType.SpecifyEvents, _configuration.SpecifyEvents);
            section.SummarizeSetting(SettingsItemType.CodeKineticModel, _configuration.CodeKineticModel);
            if (_configuration.SpecifyEvents) {
                section.SummarizeSetting(SettingsItemType.SelectedEvents, _configuration.SelectedEvents);
            }
            section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryOral, _configuration.NumberOfDosesPerDayNonDietaryOral);
            if (isAggregate) {
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryDermal, _configuration.NumberOfDosesPerDayNonDietaryDermal);
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryInhalation, _configuration.NumberOfDosesPerDayNonDietaryInhalation);
            }
            return section;
        }
    }
}
