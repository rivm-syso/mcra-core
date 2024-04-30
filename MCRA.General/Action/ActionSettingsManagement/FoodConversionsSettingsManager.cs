using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class FoodConversionsSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.FoodConversions;

        public override void initializeSettings(ProjectDto project) {
            project.GetModuleConfiguration<FoodConversionsModuleConfig>().SubstanceIndependent = true;
        }

        public override void Verify(ProjectDto project) {
            // Nothing
        }
    }
}
