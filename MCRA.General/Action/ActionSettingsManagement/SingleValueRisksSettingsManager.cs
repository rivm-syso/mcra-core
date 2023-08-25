using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class SingleValueRisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.SingleValueRisks;

        public override void initializeSettings(ProjectDto project) {
            //set default for new actions
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.EffectModelSettings.SingleValueRisksCalculationTier, false);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.EffectModelSettings.SingleValueRisksCalculationTier;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.SingleValueRisksCalculationTier:
                    project.EffectModelSettings.SingleValueRisksCalculationTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.SingleValueRiskCalculationMethod:
                    project.EffectModelSettings.SingleValueRiskCalculationMethod = Enum.Parse<SingleValueRiskCalculationMethod>(rawValue, true);
                    break;
                case SettingsItemType.RiskMetricType:
                    project.EffectModelSettings.RiskMetricType = Enum.Parse<RiskMetricType>(rawValue, true);
                    break;
                case SettingsItemType.IsInverseDistribution:
                    project.EffectModelSettings.IsInverseDistribution = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.Percentage:
                    project.EffectModelSettings.Percentage = parseDoubleSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
