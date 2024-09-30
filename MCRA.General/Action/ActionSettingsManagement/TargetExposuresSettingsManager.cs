using System.Configuration;
using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class TargetExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.TargetExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var config = project.TargetExposuresSettings;

            var cumulative = config.MultipleSubstances && config.Cumulative;

            var activeSubstancesConfig = project.ActiveSubstancesSettings;
            activeSubstancesConfig.FilterByAvailableHazardDose = cumulative;

            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
            project.AddCalculationAction(ActionType.OccurrencePatterns);
            project.AddCalculationAction(ActionType.OccurrenceFrequencies);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
        }

        public override void Verify(ProjectDto project) {
            var config = project.TargetExposuresSettings;
            config.TargetDoseLevelType = config.ExposureSources.Count > 1
                ? TargetLevelType.Internal
                : config.TargetDoseLevelType;
        }
    }
}
