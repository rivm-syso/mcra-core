using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DoseResponseData {

    public class DoseResponseDataSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.DoseResponseData;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var effectSettings = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.MergeDoseResponseExperimentsData, effectSettings.MergeDoseResponseExperimentsData);
            summarizeDataSources(project, section);
            return section;
        }
    }
}
