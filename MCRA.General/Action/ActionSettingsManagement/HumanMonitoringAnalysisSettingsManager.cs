using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HumanMonitoringAnalysisSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.ActiveSubstancesSettings.IsCompute = true;
            project.PopulationsSettings.IsCompute = true;

            var config = project.ConcentrationModelsSettings;

            var cumulative = config.MultipleSubstances && config.Cumulative;
            var activeSubstConfig = project.ActiveSubstancesSettings;
            activeSubstConfig.FilterByAvailableHazardDose = cumulative;

            var hbmConfig = project.HumanMonitoringAnalysisSettings;

            if (cumulative) {
                project.RelativePotencyFactorsSettings.IsCompute = true;
            }
            if (hbmConfig.HbmConvertToSingleTargetMatrix) {
                project.KineticModelsSettings.IsCompute = true;
            }
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
