using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HumanMonitoringAnalysisSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);

            var config = project.ConcentrationModelsSettings;

            var cumulative = config.MultipleSubstances && config.Cumulative;
            var activeSubstConfig = project.ActiveSubstancesSettings;
            activeSubstConfig.FilterByAvailableHazardDose = cumulative;

            var hbmConfig = project.HumanMonitoringAnalysisSettings;

            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
            if (hbmConfig.HbmConvertToSingleTargetMatrix) {
                project.AddCalculationAction(ActionType.KineticModels);
            }
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
