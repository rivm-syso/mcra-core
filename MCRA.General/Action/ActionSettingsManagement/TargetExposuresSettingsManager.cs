using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class TargetExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.TargetExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var config = project.TargetExposuresSettings;

            // Only change default if this is the main action 
            if (project.ActionType == ActionType) {
                config.TargetDoseLevelType = TargetLevelType.Systemic;
            }

            var cumulative = config.MultipleSubstances && config.Cumulative;

            var activeSubstancesConfig = project.ActiveSubstancesSettings;
            activeSubstancesConfig.FilterByAvailableHazardDose = cumulative;

            if (cumulative) {
                project.RelativePotencyFactorsSettings.IsCompute = true;
            }
            project.OccurrencePatternsSettings.IsCompute = true;
            project.OccurrenceFrequenciesSettings.IsCompute = true;
            project.ActiveSubstancesSettings.IsCompute = true;
            project.PopulationsSettings.IsCompute = true;
        }

        public override void Verify(ProjectDto project) {
        }
    }
}
