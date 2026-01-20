using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.OccupationalScenarios {

    [ActionType(ActionType.OccupationalScenarios)]
    public class OccupationalScenariosActionCalculator : ActionCalculatorBase<IOccupationalScenariosActionResult> {

        public OccupationalScenariosActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.OccupationalScenarioTasks][ScopingType.OccupationalScenarios].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.OccupationalScenarioTasks][ScopingType.OccupationalTasks].AlertTypeMissingData = AlertType.Notification;            
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.OccupationalScenarios = subsetManager.AllOccupationalScenarios;
            data.OccupationalTasks = subsetManager.AllOccupationalTasks;
        }

        protected override void summarizeActionResult(IOccupationalScenariosActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing occupational scenarios", 0);
            var summarizer = new OccupationalScenariosSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
