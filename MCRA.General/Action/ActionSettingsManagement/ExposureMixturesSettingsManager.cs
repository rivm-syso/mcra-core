using MCRA.General.Action.Settings.Dto;
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

        protected override string getTierSelectionEnumName() {
            return null;
        }

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
