using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Substances {

    [ActionType(ActionType.Substances)]
    public class SubstancesActionCalculator : ActionCalculatorBase<ISubstancesActionResult> {
        private SubstancesModuleConfig ModuleConfig => (SubstancesModuleConfig)_moduleSettings;

        public SubstancesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.Compounds].AllowCodesInScopeNotInSource = true;
            if (!ModuleConfig.MultipleSubstances && !ModuleConfig.IsCompute) {
                _actionDataSelectionRequirements[ScopingType.Compounds].MaxSelectionCount = 1;
            }
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (!ModuleConfig.MultipleSubstances) {
                return linkManager.GetCodesInScope(ScopingType.Compounds).Count == 1;
            } else {
                return linkManager.GetCodesInScope(ScopingType.Compounds).Count > 0;
            }
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SubstancesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var substances = subsetManager.AllCompounds;
            data.AllCompounds = substances;
            if (substances != null) {
                if (substances.Count > 1 && !ModuleConfig.MultipleSubstances) {
                    throw new Exception("Multiple substances are specified in a single substance analysis.");
                }
            }
        }

        protected override void summarizeActionResult(ISubstancesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(0);
            var summarizer = new SubstancesSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
