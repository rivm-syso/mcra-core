using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationModelsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            _ = project.CalculationActionTypes.Add(ActionType.OccurrencePatterns);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ConcentrationModelSettings.ConcentrationModelChoice, false);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.ConcentrationModelSettings.ConcentrationModelChoice;

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ConcentrationModelChoice:
                    project.ConcentrationModelSettings.ConcentrationModelChoice = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.DefaultConcentrationModel:
                    project.ConcentrationModelSettings.DefaultConcentrationModel = Enum.Parse<ConcentrationModelType>(rawValue, true);
                    break;
                case SettingsItemType.IsFallbackMrl:
                    project.ConcentrationModelSettings.IsFallbackMrl = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.FractionOfMrl:
                    project.ConcentrationModelSettings.FractionOfMrl = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.NonDetectsHandlingMethod:
                    project.ConcentrationModelSettings.NonDetectsHandlingMethod = Enum.Parse<NonDetectsHandlingMethod>(rawValue, true);
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
