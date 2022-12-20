using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {

    public sealed class OccurrenceFrequenciesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.OccurrenceFrequencies;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var cms = project.AgriculturalUseSettings;
            summarizeDataOrCompute(project, section);
            section.SummarizeSetting(SettingsItemType.SetMissingAgriculturalUseAsUnauthorized, cms.SetMissingAgriculturalUseAsUnauthorized);
            section.SummarizeSetting(SettingsItemType.UseAgriculturalUsePercentage, cms.UseAgriculturalUsePercentage);
            return section;
        }
    }
}
