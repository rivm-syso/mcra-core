using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ExposureMixturesSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.AddCalculationAction(ActionType.ActiveSubstances);
            project.AddCalculationAction(ActionType.Populations);
            project.AssessmentSettings.MultipleSubstances = true;
            var riskBased = project.MixtureSelectionSettings.ExposureApproachType == ExposureApproachType.RiskBased;
            if (riskBased) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
        }

        public override void Verify(ProjectDto project) {
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
