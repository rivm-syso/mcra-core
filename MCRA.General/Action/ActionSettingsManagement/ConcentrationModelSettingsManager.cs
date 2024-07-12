using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationModelsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            _ = project.CalculationActionTypes.Add(ActionType.OccurrencePatterns);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ActionSettings.SelectedTier, false);
        }
    }
}
