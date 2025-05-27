using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {

    public class ConsumerProductUseFrequenciesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ConsumerProductUseFrequencies;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            return section;
        }
    }
}
