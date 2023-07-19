using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {

    public sealed class OccurrenceFrequenciesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.OccurrenceFrequencies;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var settings = project.AgriculturalUseSettings;
            summarizeDataOrCompute(project, section);
            section.SummarizeSetting(SettingsItemType.OccurrencePatternsTier, settings.OccurrencePatternsTier);
            if (project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, settings.SetMissingAgriculturalUseAsUnauthorized);
            } else {
                section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, settings.UseAgriculturalUsePercentage);
            }
            return section;
        }
    }
}
