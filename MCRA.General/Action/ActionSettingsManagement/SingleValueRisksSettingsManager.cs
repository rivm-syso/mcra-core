using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class SingleValueRisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.SingleValueRisks;

        public override void initializeSettings(ProjectDto project) {
            //set default for new actions
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ActionSettings.SelectedTier, false);
        }
    }
}
