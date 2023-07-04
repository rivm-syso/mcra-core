using MCRA.General.Action.Settings.Dto;
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

        public void SetTier(ProjectDto project, RiskCalculationTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() => nameof(RiskCalculationTier);

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    Enum.TryParse(rawValue, out ExposureType exposureType);
                    project.AssessmentSettings.ExposureType = exposureType;
                    break;
                case SettingsItemType.HealthEffectType:
                    if (Enum.TryParse(rawValue, out HealthEffectType healthEffect)) {
                        project.EffectModelSettings.HealthEffectType = healthEffect;
                    }
                    break;
                case SettingsItemType.TargetDoseLevelType:
                    if (Enum.TryParse(rawValue, out TargetLevelType targetLevel)) {
                        project.EffectSettings.TargetDoseLevelType = targetLevel;
                    }
                    break;
                case SettingsItemType.RiskMetricType:
                    if (Enum.TryParse(rawValue, out RiskMetricType metricType)) {
                        project.EffectModelSettings.RiskMetricType = metricType;
                    }
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

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out RiskCalculationTier tier)) {
                project.EffectModelSettings.RiskCalculationTier = tier;
            }
        }
    }
}
