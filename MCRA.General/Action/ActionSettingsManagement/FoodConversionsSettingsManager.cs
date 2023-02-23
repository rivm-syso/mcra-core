using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class FoodConversionsSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.FoodConversions;

        public override void initializeSettings(ProjectDto project) {
            project.ConversionSettings.SubstanceIndependent = true;
        }

        public override void Verify(ProjectDto project) {
            // Nothing
        }

        protected override string getTierSelectionEnumName() {
            // No tiers
            return null;
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            throw new NotImplementedException();
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            throw new NotImplementedException();
        }
    }
}
