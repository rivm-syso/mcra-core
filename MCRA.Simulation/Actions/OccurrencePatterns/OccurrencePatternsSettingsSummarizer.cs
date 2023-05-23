using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccurrencePatterns {

    public sealed class OccurrencePatternsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.OccurrencePatterns;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var settings = project.AgriculturalUseSettings;
            summarizeDataOrCompute(project, section);
            section.SummarizeSetting(SettingsItemType.OccurrencePatternsTier, settings.OccurrencePatternsTier);
            section.SummarizeSetting(SettingsItemType.ScaleUpOccurencePatterns, settings.ScaleUpOccurencePatterns);
            if (settings.ScaleUpOccurencePatterns) {
                section.SummarizeSetting(SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses, settings.RestrictOccurencePatternScalingToAuthorisedUses, isVisible: false) ;

            }
            if (settings.UseAgriculturalUseTable && project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUseTable, settings.UseAgriculturalUseTable, isVisible: false);
                section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, settings.SetMissingAgriculturalUseAsUnauthorized);
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, settings.UseAgriculturalUsePercentage);
                if (settings.ScaleUpOccurencePatterns) {
                    section.SummarizeSetting(SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses, settings.RestrictOccurencePatternScalingToAuthorisedUses);
                }
            }
            return section;
        }
    }
}
