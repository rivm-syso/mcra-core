using MCRA.General.Action.Settings.Dto;
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

        public void SetTier(ProjectDto project, ConsumptionsTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() => nameof(ConsumptionsTier);

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out ConsumptionsTier tier)) {
                project.FoodSurveySettings.ConsumptionsTier = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
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
