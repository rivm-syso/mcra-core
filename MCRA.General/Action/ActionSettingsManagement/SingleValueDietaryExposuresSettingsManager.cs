using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class SingleValueDietaryExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.SingleValueDietaryExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.DietaryIntakeCalculationTier:
                    project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.SingleValueDietaryExposureCalculationMethod:
                    project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod = Enum.Parse<SingleValueDietaryExposuresCalculationMethod>(rawValue, true);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
