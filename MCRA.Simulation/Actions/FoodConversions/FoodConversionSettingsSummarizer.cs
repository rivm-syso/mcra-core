using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.FoodConversions {

    public sealed class FoodConversionSettingsSummarizer : ActionModuleSettingsSummarizer<FoodConversionsModuleConfig> {

        public FoodConversionSettingsSummarizer(FoodConversionsModuleConfig config): base(config) {
        }

        public override ActionType ActionType => ActionType.FoodConversions;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SubstanceIndependent, _configuration.SubstanceIndependent);
            if (_configuration.UseProcessing) {
                section.SummarizeSetting(SettingsItemType.UseProcessing, _configuration.UseProcessing);
            }
            section.SummarizeSetting(SettingsItemType.UseComposition, _configuration.UseComposition);
            if (_configuration.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.UseTdsComposition, _configuration.TotalDietStudy);
            }
            section.SummarizeSetting(SettingsItemType.UseReadAcrossFoodTranslations, _configuration.UseReadAcrossFoodTranslations);
            section.SummarizeSetting(SettingsItemType.UseMarketShares, _configuration.UseMarketShares);
            if (_configuration.UseMarketShares) {
                section.SummarizeSetting(SettingsItemType.UseSubTypes, _configuration.UseSubTypes);
            }
            section.SummarizeSetting(SettingsItemType.UseSuperTypes, _configuration.UseSuperTypes);
            section.SummarizeSetting(SettingsItemType.UseDefaultProcessingFactor, _configuration.UseDefaultProcessingFactor);
            return section;
        }
    }
}
