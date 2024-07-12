using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConcentrationModels {

    public sealed class ConcentrationModelsSettingsSummarizer : ActionModuleSettingsSummarizer<ConcentrationModelsModuleConfig> {

        public ConcentrationModelsSettingsSummarizer(ConcentrationModelsModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            section.SummarizeSetting(SettingsItemType.DefaultConcentrationModel, _configuration.DefaultConcentrationModel);
            section.SummarizeSetting(SettingsItemType.IsFallbackMrl, _configuration.IsFallbackMrl);
            if (_configuration.IsFallbackMrl) {
                section.SummarizeSetting(SettingsItemType.FractionOfMrl, _configuration.FractionOfMrl);
            }
            section.SummarizeSetting(SettingsItemType.RestrictLorImputationToAuthorisedUses, _configuration.RestrictLorImputationToAuthorisedUses);
            section.SummarizeSetting(SettingsItemType.NonDetectsHandlingMethod, _configuration.NonDetectsHandlingMethod);
            if (_configuration.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.FractionOfLor, _configuration.FractionOfLor);
            }
            section.SummarizeSetting(SettingsItemType.IsSampleBased, _configuration.IsSampleBased);
            if (_configuration.IsSampleBased) {
                section.SummarizeSetting(SettingsItemType.ImputeMissingValues, _configuration.ImputeMissingValues);
                if (_configuration.ImputeMissingValues) {
                    section.SummarizeSetting(SettingsItemType.CorrelateImputedValueWithSamplePotency, _configuration.CorrelateImputedValueWithSamplePotency);
                }
            }
            if (_configuration.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.TotalDietStudy, _configuration.TotalDietStudy);
            }
            section.SummarizeSetting(SettingsItemType.Cumulative, _configuration.Cumulative, isVisible: false);
            return section;
        }
    }
}
