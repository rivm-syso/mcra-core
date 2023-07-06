using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HumanMonitoringAnalysisSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
            var cumulative = project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative;
            project.EffectSettings.RestrictToAvailableHazardDoses = cumulative;
            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
            //Should be replace by enum KineticConversionMethod
            var useKineticConversionFactors = false;
            if (useKineticConversionFactors) {
                project.AddCalculationAction(ActionType.KineticModels);
            }
        }

        public override void Verify(ProjectDto project) {
        }

        protected override string getTierSelectionEnumName() => null;

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            // Do nothing: no tiers available.
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    Enum.TryParse(rawValue, out ExposureType exposureType);
                    project.AssessmentSettings.ExposureType = exposureType;
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
