using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ExposureMixturesSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.ActiveSubstancesSettings.IsCompute = true;
            project.PopulationsSettings.IsCompute = true;
            var cmConfig = project.ConcentrationModelsSettings;
            cmConfig.MultipleSubstances = true;

            var config = project.ExposureMixturesSettings;
            var riskBased = config.ExposureApproachType == ExposureApproachType.RiskBased;
            if (riskBased) {
                project.RelativePotencyFactorsSettings.IsCompute = true;
            }
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
