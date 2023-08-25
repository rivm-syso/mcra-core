using MCRA.General.Action.Settings;
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

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            // Nothing
        }
    }
}
