using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {
    public class ExposureBiomarkerConversionsSettingsSummarizer : ActionModuleSettingsSummarizer<ExposureBiomarkerConversionsModuleConfig> {

        public ExposureBiomarkerConversionsSettingsSummarizer(ExposureBiomarkerConversionsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.EBCSubgroupDependent, _configuration.EBCSubgroupDependent);
            return section;
        }
    }
}
