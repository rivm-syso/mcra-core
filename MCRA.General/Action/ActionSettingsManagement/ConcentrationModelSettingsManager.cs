using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationModelsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            project.CalculationActionTypes.Add(ActionType.OccurrencePatterns);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ConcentrationModelSettings.ConcentrationModelChoice, false);
        }

        public void SetTier(ProjectDto project, ConcentrationModelChoice tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() {
            return "ConcentrationModelChoice";
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out ConcentrationModelChoice tier)) {
                project.ConcentrationModelSettings.ConcentrationModelChoice = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ConcentrationModelChoice:
                    Enum.TryParse(rawValue, out ConcentrationModelChoice concentrationModelChoice);
                    project.ConcentrationModelSettings.ConcentrationModelChoice = concentrationModelChoice;
                    break;
                case SettingsItemType.DefaultConcentrationModel:
                    Enum.TryParse(rawValue, out ConcentrationModelType defaultConcentrationModel);
                    project.ConcentrationModelSettings.DefaultConcentrationModel = defaultConcentrationModel;
                    break;
                case SettingsItemType.IsFallbackMrl:
                    project.ConcentrationModelSettings.IsFallbackMrl = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.FractionOfMrl:
                    project.ConcentrationModelSettings.FractionOfMrl = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.NonDetectsHandlingMethod:
                    Enum.TryParse(rawValue, out NonDetectsHandlingMethod nonDetectsHandlingMethod);
                    project.ConcentrationModelSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
                    break;
                case SettingsItemType.FractionOfLOR:
                    project.ConcentrationModelSettings.FractionOfLOR = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.RestrictLorImputationToAuthorisedUses:
                    project.ConcentrationModelSettings.RestrictLorImputationToAuthorisedUses = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsSampleBased:
                    project.ConcentrationModelSettings.IsSampleBased = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ImputeMissingValues:
                    project.ConcentrationModelSettings.ImputeMissingValues = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CorrelateImputedValueWithSamplePotency:
                    project.ConcentrationModelSettings.CorrelateImputedValueWithSamplePotency = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseAgriculturalUseTable:
                    project.AgriculturalUseSettings.UseAgriculturalUseTable = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsParametric:
                    project.UncertaintyAnalysisSettings.IsParametric = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }
    }
}
