using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.SettingsDefinitions;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsSettingsSummarizer : ActionModuleSettingsSummarizer<RelativePotencyFactorsModuleConfig> {

        public RelativePotencyFactorsSettingsSummarizer(RelativePotencyFactorsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(_configuration.IsCompute, section);
            section.SummarizeSetting(SettingsItemType.CodeReferenceSubstance, _configuration.CodeReferenceSubstance, !string.IsNullOrEmpty(_configuration.CodeReferenceSubstance));
            return section;
        }
    }
}
