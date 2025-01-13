using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.DoseResponseData {

    public class DoseResponseDataSettingsSummarizer : ActionModuleSettingsSummarizer<DoseResponseDataModuleConfig> {

        public DoseResponseDataSettingsSummarizer(DoseResponseDataModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.MergeDoseResponseExperimentsData, _configuration.MergeDoseResponseExperimentsData);
            summarizeDataSources(project, section);
            return section;
        }
    }
}
