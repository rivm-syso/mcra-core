using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {

    [ActionType(ActionType.BiologicalMatrixConcentrationComparisons)]
    public sealed class BiologicalMatrixConcentrationComparisonsActionCalculator : ActionCalculatorBase<BiologicalMatrixConcentrationComparisonsActionResult> {
        private BiologicalMatrixConcentrationComparisonsModuleConfig ModuleConfig => (BiologicalMatrixConcentrationComparisonsModuleConfig)_moduleSettings;

        public BiologicalMatrixConcentrationComparisonsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new BiologicalMatrixConcentrationComparisonsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize();
        }

        protected override BiologicalMatrixConcentrationComparisonsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new BiologicalMatrixConcentrationComparisonsActionResult();
            var hbmTargets = data.HbmIndividualDayCollections?.Select(r => r.TargetUnit)
                ?? data.HbmIndividualCollections.Select(r => r.TargetUnit).ToList();
            if (hbmTargets.Count() > 1) {
                throw new Exception("Modelled versus HBM concentration comparisons not possible for HBM concentrations for multiple targets.");
            }
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(BiologicalMatrixConcentrationComparisonsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update($"Summarizing {ActionType.GetDisplayName(true)}", 0);
            var summarizer = new BiologicalMatrixConcentrationComparisonsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
