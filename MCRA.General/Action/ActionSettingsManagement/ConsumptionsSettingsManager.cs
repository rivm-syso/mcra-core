using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConsumptionsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.Consumptions;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.FoodSurveySettings.ConsumptionsTier, false);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.FoodSurveySettings.ConsumptionsTier;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ConsumptionsTier:
                    project.FoodSurveySettings.ConsumptionsTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExcludeIndividualsWithLessThanNDays:
                    project.SubsetSettings.ExcludeIndividualsWithLessThanNDays = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsDefaultSamplingWeight:
                    project.SubsetSettings.IsDefaultSamplingWeight = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.MinimumNumberOfDays:
                    project.SubsetSettings.MinimumNumberOfDays = parseIntSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
