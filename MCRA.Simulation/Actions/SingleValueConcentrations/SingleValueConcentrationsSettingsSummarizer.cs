using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    public class SingleValueConcentrationsSettingsSummarizer : ActionModuleSettingsSummarizer<SingleValueConcentrationsModuleConfig> {
        public SingleValueConcentrationsSettingsSummarizer(SingleValueConcentrationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(_configuration.IsCompute, section);
            section.SummarizeSetting(SettingsItemType.UseDeterministicConversionFactors, _configuration.UseDeterministicConversionFactors);
            return section;
        }
    }
}
