using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class OccurrencePatternsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.OccurrencePatterns;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            var config = project.OccurrencePatternsSettings;
            SetTier(project, config.OccurrencePatternsTier, false);
        }
    }
}
