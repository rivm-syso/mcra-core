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
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);

            if (_configuration.TargetDoseLevelType == TargetLevelType.External) {
                section.SummarizeSetting(SettingsItemType.ExposureRoutes, _configuration.ExposureRoutes);
            }

            if (!isCompute) {
                section.SummarizeSetting(SettingsItemType.RestrictToCriticalEffect, _configuration.RestrictToCriticalEffect);
                section.SummarizeSetting(SettingsItemType.HCSubgroupDependent, _configuration.HCSubgroupDependent);
            }

            if (isCompute) {
                section.SummarizeSetting(SettingsItemType.TargetDosesCalculationMethod, _configuration.TargetDosesCalculationMethod);
                if (_configuration.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds) {
                    section.SummarizeSetting(SettingsItemType.UseBMDL, _configuration.UseBMDL);
                }
                if (_configuration.PointOfDeparture != PointOfDeparture.FromReference) {
                    section.SummarizeSetting(SettingsItemType.PointOfDeparture, _configuration.PointOfDeparture);
                }
                section.SummarizeSetting(SettingsItemType.TargetDoseSelectionMethod, _configuration.TargetDoseSelectionMethod);

                section.SummarizeSetting(SettingsItemType.ApplyKineticConversions, _configuration.ApplyKineticConversions);
                if (_configuration.ApplyKineticConversions) {
                    section.SummarizeSetting(SettingsItemType.InternalModelType, _configuration.InternalModelType);
                    section.SummarizeSetting(SettingsItemType.HazardCharacterisationsConvertToSingleTargetMatrix, _configuration.HazardCharacterisationsConvertToSingleTargetMatrix);
                    if (_configuration.TargetDoseLevelType == TargetLevelType.Internal
                        && _configuration.HazardCharacterisationsConvertToSingleTargetMatrix
                    ) {
                        section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.TargetMatrix.GetDisplayName());
                    }
                }

                section.SummarizeSetting(SettingsItemType.UseInterSpeciesConversionFactors, _configuration.UseInterSpeciesConversionFactors);
                section.SummarizeSetting(SettingsItemType.UseIntraSpeciesConversionFactors, _configuration.UseIntraSpeciesConversionFactors);
                section.SummarizeSetting(SettingsItemType.UseAdditionalAssessmentFactor, _configuration.UseAdditionalAssessmentFactor);
                if (_configuration.UseAdditionalAssessmentFactor) {
                    section.SummarizeSetting(SettingsItemType.AdditionalAssessmentFactor, _configuration.AdditionalAssessmentFactor);
                }

                section.SummarizeSetting(SettingsItemType.ImputeMissingHazardDoses, _configuration.ImputeMissingHazardDoses);
                if (_configuration.ImputeMissingHazardDoses) {
                    section.SummarizeSetting(SettingsItemType.HazardDoseImputationMethod, _configuration.HazardDoseImputationMethod);
                }
            }
            return section;
        }
    }
}
