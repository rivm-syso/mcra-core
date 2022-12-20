using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConcentrationDistributions {

    public class ConcentrationDistributionsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ConcentrationDistributions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            return section;
        }
    }
}
