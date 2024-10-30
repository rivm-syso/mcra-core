using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class EnvironmentalBurdenOfDiseaseSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.EnvironmentalBurdenOfDisease;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
