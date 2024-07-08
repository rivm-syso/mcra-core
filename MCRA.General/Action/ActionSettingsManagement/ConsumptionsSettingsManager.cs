using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConsumptionsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.Consumptions;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            var config = project.ConsumptionsSettings;
            SetTier(project, config.ConsumptionsTier, false);
        }
    }
}
