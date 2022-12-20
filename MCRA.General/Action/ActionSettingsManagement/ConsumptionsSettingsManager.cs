using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConsumptionsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.Consumptions;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }

        protected override string getTierSelectionEnumName() {
            //no tiers
            return null;
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
        }
    }
}
