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
