using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationModelsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            _ = project.OccurrencePatternsSettings.IsCompute = true;
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ActionSettings.SelectedTier, ActionType);
        }
    }
}
