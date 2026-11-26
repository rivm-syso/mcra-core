using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConsumerProductExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConsumerProductExposures;

        public override void initializeSettings(ProjectDto project) {
            if (project.ConsumerProductExposuresSettings.SelectedExposureRoutes?.Count == 0) {
                project.ConsumerProductExposuresSettings.SelectedExposureRoutes = [ExposureRoute.Dermal, ExposureRoute.Inhalation, ExposureRoute.Oral];
            }
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
