using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {

    [ActionType(ActionType.BiologicalMatrixConcentrationComparisons)]
    public sealed class BiologicalMatrixConcentrationComparisonsActionCalculator : ActionCalculatorBase<BiologicalMatrixConcentrationComparisonsActionResult> {

        public BiologicalMatrixConcentrationComparisonsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new BiologicalMatrixConcentrationComparisonsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override BiologicalMatrixConcentrationComparisonsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new BiologicalMatrixConcentrationComparisonsActionResult() ;
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(BiologicalMatrixConcentrationComparisonsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update($"Summarizing {ActionType.GetDisplayName(true)}", 0);
            var summarizer = new BiologicalMatrixConcentrationComparisonsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
