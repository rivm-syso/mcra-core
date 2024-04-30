using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {

    public sealed class OccurrenceFrequenciesSettingsSummarizer : ActionModuleSettingsSummarizer<OccurrenceFrequenciesModuleConfig> {

        public OccurrenceFrequenciesSettingsSummarizer(OccurrenceFrequenciesModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(isCompute, section);
            section.SummarizeSetting(SettingsItemType.OccurrencePatternsTier, _configuration.OccurrencePatternsTier);
            if (isCompute) {
                section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, _configuration.SetMissingAgriculturalUseAsUnauthorized);
            } else {
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, _configuration.UseAgriculturalUsePercentage);
            }
            return section;
        }
    }
}
