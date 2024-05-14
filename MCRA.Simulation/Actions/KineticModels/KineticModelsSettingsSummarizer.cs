using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.KineticModels {

    public class KineticModelsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.KineticModels;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var nds = project.NonDietarySettings;
            var km = project.KineticModelSettings;
            var isAggregate = project.AssessmentSettings.Aggregate;
            var isKineticConversionModel = km.InternalModelType == InternalModelType.AbsorptionFactorModel;
            section.SummarizeSetting(SettingsItemType.InternalModelType, km.InternalModelType);
            if (!isKineticConversionModel) {
                section.SummarizeSetting(SettingsItemType.CodeCompartment, project.KineticModelSettings.CodeCompartment);
                section.SummarizeSetting(SettingsItemType.UseParameterVariability, km.UseParameterVariability);
                section.SummarizeSetting(SettingsItemType.NumberOfDays, km.NumberOfDays);
                section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDay, km.NumberOfDosesPerDay);
                section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, km.NonStationaryPeriod);
                section.SummarizeSetting(SettingsItemType.SpecifyEvents, km.SpecifyEvents);
                if (km.SpecifyEvents) {
                    section.SummarizeSetting(SettingsItemType.SelectedEvents, km.SelectedEvents);
                }
            }
            section.SummarizeSetting(SettingsItemType.OralAbsorptionFactorForDietaryExposure, nds.OralAbsorptionFactorForDietaryExposure);
            if (isAggregate) {
                section.SummarizeSetting(SettingsItemType.OralAbsorptionFactor, nds.OralAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryOral, km.NumberOfDosesPerDayNonDietaryOral);
                }
                section.SummarizeSetting(SettingsItemType.DermalAbsorptionFactor, nds.DermalAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryDermal, km.NumberOfDosesPerDayNonDietaryDermal);
                }
                section.SummarizeSetting(SettingsItemType.InhalationAbsorptionFactor, nds.InhalationAbsorptionFactor);
                if (!isKineticConversionModel) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryInhalation, km.NumberOfDosesPerDayNonDietaryInhalation);
                }
            }
            section.SummarizeSetting(SettingsItemType.KCFSubgroupDependent, km.KCFSubgroupDependent);

            return section;
        }
    }
}
