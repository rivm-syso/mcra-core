using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProductExposureDeterminants {

    [ActionType(ActionType.ConsumerProductExposureDeterminants)]
    public class ConsumerProductExposureDeterminantsActionCalculator : ActionCalculatorBase<IConsumerProductExposureDeterminantsActionResult> {

        public ConsumerProductExposureDeterminantsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ConsumerProductApplicationAmounts][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductExposureFractions][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.ConsumerProductExposureFractions = [.. subsetManager.AllConsumerProductExposureFractions];
            data.ConsumerProductApplicationAmounts = [.. subsetManager.AllConsumerProductApplicationAmounts];
        }

        protected override void summarizeActionResult(IConsumerProductExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing consumer product exposure determinants", 0);
            var summarizer = new ConsumerProductExposureDeterminantsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
