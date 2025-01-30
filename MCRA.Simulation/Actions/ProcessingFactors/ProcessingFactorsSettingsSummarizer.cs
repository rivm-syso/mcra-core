using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ProcessingFactors {
    public class ProcessingFactorsSettingsSummarizer : ActionModuleSettingsSummarizer<ProcessingFactorsModuleConfig> {
        public ProcessingFactorsSettingsSummarizer(ProcessingFactorsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.IsDistribution, _configuration.IsDistribution, isVisible: false);
            section.SummarizeSetting(SettingsItemType.AllowHigherThanOne, _configuration.AllowHigherThanOne, isVisible: false);
            section.SummarizeSetting(SettingsItemType.DefaultMissingProcessingFactor, _configuration.DefaultMissingProcessingFactor, isVisible: false);
            return section;
        }
    }
}
