using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class OccurrencePatternsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.OccurrencePatterns;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ActionSettings.SelectedTier, false);
        }
    }
}
