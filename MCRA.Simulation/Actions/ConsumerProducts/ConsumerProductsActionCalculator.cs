using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProducts {

    [ActionType(ActionType.ConsumerProducts)]
    public sealed class ConsumerProductsActionCalculator : ActionCalculatorBase<IConsumerProductsActionResult> {

        public ConsumerProductsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.ConsumerProducts].AllowCodesInScopeNotInSource = true;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumerProductsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AllConsumerProducts = subsetManager.AllConsumerProducts;
        }

        protected override void summarizeActionResult(IConsumerProductsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new ConsumerProductsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
