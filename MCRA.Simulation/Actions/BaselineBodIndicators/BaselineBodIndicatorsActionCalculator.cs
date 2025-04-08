using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.BaselineBodIndicators {

    [ActionType(ActionType.BaselineBodIndicators)]
    public class BaselineBodIndicatorsActionCalculator : ActionCalculatorBase<IBaselineBodIndicatorsActionResult> {

        public BaselineBodIndicatorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.BaselineBodIndicators][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.BaselineBodIndicators][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.BaselineBodIndicators = subsetManager.AllBaselineBodIndicators
                .Where(r => r.Population == null
                    || data.SelectedPopulation.Code == "Generated"
                    || r.Population == data.SelectedPopulation
                )
                .ToList();
        }

        protected override void summarizeActionResult(IBaselineBodIndicatorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new BaselineBodIndicatorsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
