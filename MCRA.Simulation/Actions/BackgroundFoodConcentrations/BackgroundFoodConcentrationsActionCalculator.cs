using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.BackgroundFoodConcentrations {

    [ActionType(ActionType.BackgroundFoodConcentrations)]
    public class BackgroundFoodConcentrationsActionCalculator : ActionCalculatorBase<IBackgroundFoodConcentrationsActionResult> {

        public BackgroundFoodConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.BackgroundFoodConcentrations][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.BackgroundFoodConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            data.BackgroundFoodConcentrations = subsetManager.AllBackgroundFoodConcentrations;
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IBackgroundFoodConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new BackgroundFoodConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
