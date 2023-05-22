using MCRA.General.Action.Settings.Dto;
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

        public void SetTier(ProjectDto project, SingleValueRisksCalculationTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() => "SingleValueRisksCalculationTier";

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.SingleValueRiskCalculationMethod:
                    if (Enum.TryParse(rawValue, out SingleValueRiskCalculationMethod calculationMethod)) {
                        project.EffectModelSettings.SingleValueRiskCalculationMethod = calculationMethod;
                    }
                    break;
                case SettingsItemType.RiskMetricType:
                    if (Enum.TryParse(rawValue, out RiskMetricType metricType)) {
                        project.EffectModelSettings.RiskMetricType = metricType;
                    }
                    break;
                case SettingsItemType.IsInverseDistribution:
                    project.EffectModelSettings.IsInverseDistribution = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out SingleValueRisksCalculationTier tier)) {
                project.EffectModelSettings.SingleValueRisksCalculationTier = tier;
            }
        }
    }
}
