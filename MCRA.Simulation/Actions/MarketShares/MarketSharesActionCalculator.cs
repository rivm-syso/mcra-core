using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.MarketShares {

    [ActionType(ActionType.MarketShares)]
    public class MarketSharesActionCalculator : ActionCalculatorBase<IMarketSharesActionResult> {

        public MarketSharesActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.MarketShares][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.MarketShares = subsetManager.AllMarketShares;
        }

        protected override void summarizeActionResult(IMarketSharesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new MarketSharesSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
