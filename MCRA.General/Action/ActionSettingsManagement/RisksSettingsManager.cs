using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class RisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.Risks;

        public override void initializeSettings(ProjectDto project) {
            project.AddCalculationAction(ActionType.Populations);
        }

        public override void Verify(ProjectDto project) {
            var config = project.RisksSettings;
            SetTier(project, config.RiskCalculationTier, false);
        }
    }
}
