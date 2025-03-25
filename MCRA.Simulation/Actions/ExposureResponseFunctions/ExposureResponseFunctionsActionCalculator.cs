using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {

    [ActionType(ActionType.ExposureResponseFunctions)]
    public class ExposureResponseFunctionsActionCalculator : ActionCalculatorBase<IExposureResponseFunctionsActionResult> {

        public ExposureResponseFunctionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ExposureResponseFunctions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ExposureResponseFunctions][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var exposureResponseFunctions = subsetManager.AllExposureResponseFunctions;
            data.ExposureResponseFunctions = [.. exposureResponseFunctions];
        }

        protected override void summarizeActionResult(IExposureResponseFunctionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureResponseFunctionsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
