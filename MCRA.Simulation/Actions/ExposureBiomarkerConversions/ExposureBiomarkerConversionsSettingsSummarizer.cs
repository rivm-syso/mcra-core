using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {
    public class ExposureBiomarkerConversionsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ExposureBiomarkerConversions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var ebc = project.ExposureBiomarkerConversionsSettings;
            section.SummarizeSetting(SettingsItemType.EBCSubgroupDependent, ebc.EBCSubgroupDependent);
            return section;
        }
    }
}
