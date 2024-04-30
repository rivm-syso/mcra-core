using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.Concentrations;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
