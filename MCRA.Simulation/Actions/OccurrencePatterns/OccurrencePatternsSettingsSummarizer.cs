using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.OccurrencePatterns {

    public sealed class OccurrencePatternsSettingsSummarizer : ActionModuleSettingsSummarizer<OccurrencePatternsModuleConfig> {

        public OccurrencePatternsSettingsSummarizer(OccurrencePatternsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(_configuration.IsCompute, section);
            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            if (_configuration.IsCompute) {
                section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, _configuration.SetMissingAgriculturalUseAsUnauthorized);
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, _configuration.UseAgriculturalUsePercentage);
                if (_configuration.UseAgriculturalUsePercentage) {
                    section.SummarizeSetting(SettingsItemType.ScaleUpOccurencePatterns, _configuration.ScaleUpOccurencePatterns);
                    if (_configuration.ScaleUpOccurencePatterns) {
                        section.SummarizeSetting(SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses, _configuration.RestrictOccurencePatternScalingToAuthorisedUses);
                    }
                }
            }
            return section;
        }
    }
}
