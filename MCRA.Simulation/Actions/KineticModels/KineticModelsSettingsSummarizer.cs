using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.KineticModels {

    public class KineticModelsSettingsSummarizer : ActionModuleSettingsSummarizer<KineticModelsModuleConfig> {

        public KineticModelsSettingsSummarizer(KineticModelsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var isAggregate = base._configuration.Aggregate;
            var isKineticConversionModel = _configuration.InternalModelType == InternalModelType.AbsorptionFactorModel;
            section.SummarizeSetting(SettingsItemType.InternalModelType, _configuration.InternalModelType);
            if (!isKineticConversionModel) {
                section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.CodeCompartment);
                section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
                section.SummarizeSetting(SettingsItemType.NumberOfDays, _configuration.NumberOfDays);
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDay, _configuration.NumberOfDosesPerDay);
                section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
                section.SummarizeSetting(SettingsItemType.SpecifyEvents, _configuration.SpecifyEvents);
                if (_configuration.SpecifyEvents) {
                    section.SummarizeSetting(SettingsItemType.SelectedEvents, _configuration.SelectedEvents);
                }
            }
            section.SummarizeSetting(SettingsItemType.OralAbsorptionFactorForDietaryExposure, _configuration.OralAbsorptionFactorForDietaryExposure);
            if (isAggregate) {
                section.SummarizeSetting(SettingsItemType.OralAbsorptionFactor, _configuration.OralAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryOral, _configuration.NumberOfDosesPerDayNonDietaryOral);
                }
                section.SummarizeSetting(SettingsItemType.DermalAbsorptionFactor, _configuration.DermalAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryDermal, _configuration.NumberOfDosesPerDayNonDietaryDermal);
                }
                section.SummarizeSetting(SettingsItemType.InhalationAbsorptionFactor, _configuration.InhalationAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryInhalation, _configuration.NumberOfDosesPerDayNonDietaryInhalation);
                }
            }
            section.SummarizeSetting(SettingsItemType.KCFSubgroupDependent, _configuration.KCFSubgroupDependent);

            return section;
        }
    }
}
