using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {

    public class SingleValueNonDietaryExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<SingleValueNonDietaryExposuresModuleConfig> {

        public SingleValueNonDietaryExposuresSettingsSummarizer(SingleValueNonDietaryExposuresModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            section.SummarizeSetting(SettingsItemType.CodeConfiguration, _configuration.CodeConfiguration);
            return section;
        }
    }
}
