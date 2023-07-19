using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FoodExtrapolations
{

    [ActionType(ActionType.FoodExtrapolations)]
    public class FoodExtrapolationsActionCalculator : ActionCalculatorBase<IFoodExtrapolationsActionResult> {

        public FoodExtrapolationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.FoodExtrapolations][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.FoodExtrapolations = subsetManager.AllFoodExtrapolations;
        }

        protected override void summarizeActionResult(IFoodExtrapolationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new FoodExtrapolationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
