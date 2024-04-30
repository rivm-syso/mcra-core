using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public class HazardCharacterisationsSettingsSummarizer : ActionModuleSettingsSummarizer<HazardCharacterisationsModuleConfig> {

        public HazardCharacterisationsSettingsSummarizer(HazardCharacterisationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(isCompute, section);
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.Aggregate, _configuration.Aggregate);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.PointOfDeparture, _configuration.PointOfDeparture);
            if (!isCompute) {
                section.SummarizeSetting(SettingsItemType.RestrictToCriticalEffect, _configuration.RestrictToCriticalEffect);
                section.SummarizeSetting(SettingsItemType.HCSubgroupDependent, _configuration.HCSubgroupDependent);
            }
            if (_configuration.HazardCharacterisationsConvertToSingleTargetMatrix) {
                section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.TargetMatrix.GetDisplayName());
            }

            if (isCompute) {
                section.SummarizeSetting(SettingsItemType.TargetDosesCalculationMethod, _configuration.TargetDosesCalculationMethod);

                section.SummarizeSetting(SettingsItemType.TargetDoseSelectionMethod, _configuration.TargetDoseSelectionMethod);
                section.SummarizeSetting(SettingsItemType.ImputeMissingHazardDoses, _configuration.ImputeMissingHazardDoses);
                if (_configuration.ImputeMissingHazardDoses) {
                    section.SummarizeSetting(SettingsItemType.HazardDoseImputationMethod, _configuration.HazardDoseImputationMethod);
                }
                section.SummarizeSetting(SettingsItemType.UseDoseResponseModels, _configuration.UseDoseResponseModels);
                if (_configuration.UseDoseResponseModels) {
                    section.SummarizeSetting(SettingsItemType.UseBMDL, _configuration.UseBMDL);
                }
                section.SummarizeSetting(SettingsItemType.UseAdditionalAssessmentFactor, _configuration.UseAdditionalAssessmentFactor);
                if (_configuration.UseAdditionalAssessmentFactor) {
                    section.SummarizeSetting(SettingsItemType.AdditionalAssessmentFactor, _configuration.AdditionalAssessmentFactor);
                }
                section.SummarizeSetting(SettingsItemType.UseInterSpeciesConversionFactors, _configuration.UseInterSpeciesConversionFactors);
                section.SummarizeSetting(SettingsItemType.UseIntraSpeciesConversionFactors, _configuration.UseIntraSpeciesConversionFactors);
            }
            return section;
        }
    }
}
