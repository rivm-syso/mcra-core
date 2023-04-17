using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Substances {

    [ActionType(ActionType.Substances)]
    public class SubstancesActionCalculator : ActionCalculatorBase<ISubstancesActionResult> {

        public SubstancesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.Compounds].AllowCodesInScopeNotInSource = true;
            if (!_project.AssessmentSettings.MultipleSubstances
                && (!_project.LoopScopingTypes?.Contains(ScopingType.Compounds) ?? true)
            ) {
                _actionDataSelectionRequirements[ScopingType.Compounds].MaxSelectionCount = 1;
            }
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (!_project.AssessmentSettings.MultipleSubstances) {
                return linkManager.GetCodesInScope(ScopingType.Compounds).Count == 1;
            } else {
                return linkManager.GetCodesInScope(ScopingType.Compounds).Count > 0;
            }
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SubstancesSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var substances = subsetManager.AllCompounds;
            data.AllCompounds = substances;
            if (substances != null) {
                if (substances.Count > 1 && !_project.AssessmentSettings.MultipleSubstances) {
                    throw new Exception("Multiple substances are specified in a single substance analysis is specified.");
                }
            }
        }

        protected override void summarizeActionResult(ISubstancesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(0);
            var summarizer = new SubstancesSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
