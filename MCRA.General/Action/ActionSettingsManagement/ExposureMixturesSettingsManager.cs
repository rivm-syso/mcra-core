using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ExposureMixturesSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
            var cmConfig = project.GetModuleConfiguration<ConcentrationModelsModuleConfig>();
            cmConfig.MultipleSubstances = true;

            var config = project.GetModuleConfiguration<ExposureMixturesModuleConfig>();
            var riskBased = config.ExposureApproachType == ExposureApproachType.RiskBased;
            if (riskBased) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
