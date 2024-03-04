using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class RisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.Risks;

        public override void initializeSettings(ProjectDto project) {
            //set default for new actions
            project.RisksSettings.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            project.AddCalculationAction(ActionType.Populations);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.RisksSettings.RiskCalculationTier, false);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.RisksSettings.RiskCalculationTier;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.RiskCalculationTier:
                    project.RisksSettings.RiskCalculationTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.HealthEffectType:
                    project.RisksSettings.HealthEffectType = Enum.Parse<HealthEffectType>(rawValue, true);
                    break;
                case SettingsItemType.TargetDoseLevelType:
                    project.EffectSettings.TargetDoseLevelType = Enum.Parse<TargetLevelType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureCalculationMethod:
                    project.AssessmentSettings.ExposureCalculationMethod = Enum.Parse<ExposureCalculationMethod>(rawValue, true);
                    break;
                case SettingsItemType.RiskMetricType:
                    project.RisksSettings.RiskMetricType = Enum.Parse<RiskMetricType>(rawValue, true);
                    break;
                case SettingsItemType.MultipleSubstances:
                    project.AssessmentSettings.MultipleSubstances = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CumulativeRisk:
                    project.RisksSettings.CumulativeRisk = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsInverseDistribution:
                    project.RisksSettings.IsInverseDistribution = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ThresholdMarginOfExposure:
                    project.RisksSettings.ThresholdMarginOfExposure = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.LeftMargin:
                    project.RisksSettings.LeftMargin = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.RightMargin:
                    project.RisksSettings.RightMargin = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.ConfidenceInterval:
                    project.RisksSettings.ConfidenceInterval = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.NumberOfLabels:
                    project.RisksSettings.NumberOfLabels = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.NumberOfSubstances:
                    project.RisksSettings.NumberOfSubstances = parseIntSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
