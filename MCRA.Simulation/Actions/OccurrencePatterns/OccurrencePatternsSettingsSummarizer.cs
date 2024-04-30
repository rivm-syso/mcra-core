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

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(isCompute, section);
            section.SummarizeSetting(SettingsItemType.OccurrencePatternsTier, _configuration.OccurrencePatternsTier);
            if (isCompute) {
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
