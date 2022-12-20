using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Foods {

    public class FoodsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Foods;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            return section;
        }
    }
}
