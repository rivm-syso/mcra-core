﻿using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HazardCharacterisationsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            if (project.EffectSettings.TargetDosesCalculationMethod != TargetDosesCalculationMethod.InVivoPods) {
                project.EffectSettings.UseDoseResponseModels = true;
            }
            if (project.ActionType == ActionType.HazardCharacterisations || project.ActionType == ActionType.RelativePotencyFactors) {
                project.AssessmentSettings.Aggregate = project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.Aggregate:
                    project.AssessmentSettings.Aggregate = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.TargetDoseLevelType:
                    project.EffectSettings.TargetDoseLevelType = Enum.Parse<TargetLevelType>(rawValue, true);
                    break;
                case SettingsItemType.UseDoseResponseModels:
                    project.EffectSettings.UseDoseResponseModels = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseInterSpeciesConversionFactors:
                    project.EffectSettings.UseInterSpeciesConversionFactors = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseIntraSpeciesConversionFactors:
                    project.EffectSettings.UseIntraSpeciesConversionFactors = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseAdditionalAssessmentFactor:
                    project.EffectSettings.UseAdditionalAssessmentFactor = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.AdditionalAssessmentFactor:
                    project.EffectSettings.AdditionalAssessmentFactor = parseDoubleSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }

        public static List<HazardDoseImputationMethodType> AvailableHazardDoseImputationMethodTypes(bool hasHazardData) {
            var result = new List<HazardDoseImputationMethodType>() {
                    HazardDoseImputationMethodType.MunroP5,
                    HazardDoseImputationMethodType.MunroUnbiased,
                };
            if (hasHazardData) {
                result.Add(HazardDoseImputationMethodType.HazardDosesP5);
                result.Add(HazardDoseImputationMethodType.HazardDosesUnbiased);
            }
            return result;
        }
    }
}
