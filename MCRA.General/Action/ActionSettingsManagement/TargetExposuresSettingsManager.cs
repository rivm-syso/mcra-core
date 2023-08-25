using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class TargetExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.TargetExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var cumulative = project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative;
            project.EffectSettings.RestrictToAvailableHazardDoses = cumulative;
            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
            project.AddCalculationAction(ActionType.OccurrencePatterns);
            project.AddCalculationAction(ActionType.OccurrenceFrequencies);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
        }

        public override void Verify(ProjectDto project) {
            project.EffectSettings.TargetDoseLevelType = project.AssessmentSettings.Aggregate
                ? TargetLevelType.Internal
                : project.EffectSettings.TargetDoseLevelType;
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.Aggregate:
                    project.AssessmentSettings.Aggregate = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.TargetDoseLevelType:
                    project.EffectSettings.TargetDoseLevelType = Enum.Parse<TargetLevelType>(rawValue, true);
                    break;
                case SettingsItemType.MatchSpecificIndividuals:
                    project.NonDietarySettings.MatchSpecificIndividuals = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsCorrelationBetweenIndividuals:
                    project.NonDietarySettings.IsCorrelationBetweenIndividuals = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
