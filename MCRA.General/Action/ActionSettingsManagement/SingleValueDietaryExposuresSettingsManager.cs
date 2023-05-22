using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class SingleValueDietaryExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.SingleValueDietaryExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }

        protected override string getTierSelectionEnumName() {
            //no tiers
            return null;
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out DietaryIntakeCalculationTier tier)) {
                project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    Enum.TryParse(rawValue, out ExposureType exposureType);
                    project.AssessmentSettings.ExposureType = exposureType;
                    break;
                case SettingsItemType.SingleValueDietaryExposureCalculationMethod:
                    Enum.TryParse(rawValue, out SingleValueDietaryExposuresCalculationMethod singleValueDietaryExposuresCalculationMethod);
                    project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod = singleValueDietaryExposuresCalculationMethod;
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
