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
            var cms = project.AgriculturalUseSettings;
            summarizeDataOrCompute(project, section);
            section.SummarizeSetting(SettingsItemType.ScaleUpOccurencePatterns, cms.ScaleUpOccurencePatterns);
            if (cms.ScaleUpOccurencePatterns) {
                section.SummarizeSetting(SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses, cms.RestrictOccurencePatternScalingToAuthorisedUses, isVisible: false) ;

            }
            if (cms.UseAgriculturalUseTable && project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUseTable, cms.UseAgriculturalUseTable, isVisible: false);
                section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, cms.SetMissingAgriculturalUseAsUnauthorized);
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, cms.UseAgriculturalUsePercentage);
                if (cms.ScaleUpOccurencePatterns) {
                    section.SummarizeSetting(SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses, cms.RestrictOccurencePatternScalingToAuthorisedUses);
                }
            }
            return section;
        }
    }
}
