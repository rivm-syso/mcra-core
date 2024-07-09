using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ExposureMixturesSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
            var cmConfig = project.ConcentrationModelsSettings;
            cmConfig.MultipleSubstances = true;

            var config = project.ExposureMixturesSettings;
            var riskBased = config.McrExposureApproachType == ExposureApproachType.RiskBased;
            if (riskBased) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
