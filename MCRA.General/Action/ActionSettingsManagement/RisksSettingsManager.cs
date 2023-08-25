using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class RisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.Risks;

        public override void initializeSettings(ProjectDto project) {
            //set default for new actions
            project.EffectModelSettings.RiskMetricType = RiskMetricType.HazardIndex;
            project.AddCalculationAction(ActionType.Populations);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.EffectModelSettings.RiskCalculationTier, false);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.EffectModelSettings.RiskCalculationTier;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.RiskCalculationTier:
                    project.EffectModelSettings.RiskCalculationTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.HealthEffectType:
                    project.EffectModelSettings.HealthEffectType = Enum.Parse<HealthEffectType>(rawValue, true);
                    break;
                case SettingsItemType.TargetDoseLevelType:
                    project.EffectSettings.TargetDoseLevelType = Enum.Parse<TargetLevelType>(rawValue, true);
                    break;
                case SettingsItemType.RiskMetricType:
                    project.EffectModelSettings.RiskMetricType = Enum.Parse<RiskMetricType>(rawValue, true);
                    break;
                case SettingsItemType.MultipleSubstances:
                    project.AssessmentSettings.MultipleSubstances = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CumulativeRisk:
                    project.EffectModelSettings.CumulativeRisk = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsInverseDistribution:
                    project.EffectModelSettings.IsInverseDistribution = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ThresholdMarginOfExposure:
                    project.EffectModelSettings.ThresholdMarginOfExposure = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.LeftMargin:
                    project.EffectModelSettings.LeftMargin = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.RightMargin:
                    project.EffectModelSettings.RightMargin = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.ConfidenceInterval:
                    project.EffectModelSettings.ConfidenceInterval = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.NumberOfLabels:
                    project.EffectModelSettings.NumberOfLabels = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.NumberOfSubstances:
                    project.EffectModelSettings.NumberOfSubstances = parseIntSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
