using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.OccupationalTaskExposures {

    [ActionType(ActionType.OccupationalTaskExposures)]
    public class OccupationalTaskExposuresActionCalculator : ActionCalculatorBase<IOccupationalTaskExposuresActionResult> {

        public OccupationalTaskExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.OccupationalTaskExposures][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.OccupationalTaskExposures][ScopingType.OccupationalTasks].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var tasks = data.OccupationalScenarios.Values
                .SelectMany(c => c.Tasks)
                .Select(c => c.OccupationalTask)
                .ToHashSet();

            data.OccupationalTaskExposures = [.. subsetManager.AllOccupationalTaskExposures.Where(c => tasks.Contains(c.OccupationalTask))];
        }

        protected override void summarizeActionResult(IOccupationalTaskExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing occupational scenarios", 0);
            var summarizer = new OccupationalTaskExposuresSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
