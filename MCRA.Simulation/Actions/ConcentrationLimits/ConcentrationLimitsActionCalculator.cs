using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ConcentrationLimits {

    [ActionType(ActionType.ConcentrationLimits)]
    public class ConcentrationLimitsActionCalculator : ActionCalculatorBase<IConcentrationLimitsActionResult> {

        public ConcentrationLimitsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ConcentrationLimits][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationLimits][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.MaximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit>();
            foreach (var record in subsetManager.AllMaximumConcentrationLimits) {
                data.MaximumConcentrationLimits.Add((record.Food, record.Compound), record);
            }
        }

        protected override void summarizeActionResult(IConcentrationLimitsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConcentrationLimitsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
