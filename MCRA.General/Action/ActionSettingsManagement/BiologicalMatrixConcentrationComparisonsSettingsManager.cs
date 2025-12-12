using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class BiologicalMatrixConcentrationComparisonsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.BiologicalMatrixConcentrationComparisons;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var config = project.BiologicalMatrixConcentrationComparisonsSettings;
            config.TargetDoseLevelType = TargetLevelType.Systemic;
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
