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
            //nothing
        }

        protected override string getTierSelectionEnumName() {
            //no tiers
            return null;
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
        }
    }
}
