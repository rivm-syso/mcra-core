using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.TestSystems {
    [ActionType(ActionType.TestSystems)]
    public class TestSystemsActionCalculator : ActionCalculatorBase<ITestSystemsActionResult> {

        public TestSystemsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new TestSystemsSettingsSummarizer();
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.TestSystems = subsetManager.AllTestSystems;
        }

        protected override void summarizeActionResult(ITestSystemsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new TestSystemsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}