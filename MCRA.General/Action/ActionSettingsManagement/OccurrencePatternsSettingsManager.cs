using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class OccurrencePatternsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.OccurrencePatterns;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.AgriculturalUseSettings.OccurrencePatternsTier, false);
        }

        public void SetTier(ProjectDto project, OccurrencePatternsTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() => nameof(OccurrencePatternsTier);

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out OccurrencePatternsTier tier)) {
                project.AgriculturalUseSettings.OccurrencePatternsTier = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses:
                    project.AgriculturalUseSettings.RestrictOccurencePatternScalingToAuthorisedUses = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ScaleUpOccurencePatterns:
                    project.AgriculturalUseSettings.ScaleUpOccurencePatterns = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseAgriculturalUsePercentage:
                    project.AgriculturalUseSettings.UseAgriculturalUsePercentage = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.SetMissingAgriculturalUseAsUnauthorized:
                    project.AgriculturalUseSettings.SetMissingAgriculturalUseAsUnauthorized = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
