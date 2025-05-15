using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.PbkModels {

    public class PbkModelsSettingsSummarizer : ActionModuleSettingsSummarizer<PbkModelsModuleConfig> {

        public PbkModelsSettingsSummarizer(PbkModelsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
            return section;
        }
    }
}
