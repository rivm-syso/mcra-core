using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Effects {

    [ActionType(ActionType.Effects)]
    public class EffectsActionCalculator : ActionCalculatorBase<IEffectsActionResult> {
        private EffectsModuleConfig ModuleConfig => (EffectsModuleConfig)_moduleSettings;

        public EffectsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.Effects].AllowCodesInScopeNotInSource = true;
            if (!ModuleConfig.MultipleEffects
                && !ModuleConfig.IncludeAopNetwork
                && !IsLoopScope(ScopingType.Effects)
            ) {
                _actionDataSelectionRequirements[ScopingType.Effects].MaxSelectionCount = 1;
            }
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (ModuleConfig.MultipleEffects) {
                return true;
            } else if (ModuleConfig.IncludeAopNetwork) {
                if (!string.IsNullOrEmpty(ModuleConfig.CodeFocalEffect)) {
                    return linkManager.GetCodesInScope(ScopingType.Effects).Contains(ModuleConfig.CodeFocalEffect);
                }
                return false;
            } else {
                return linkManager.GetCodesInScope(ScopingType.Effects).Count == 1;
            }
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new EffectsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AllEffects = subsetManager.AllEffects.Values;
            data.SelectedEffect = !ModuleConfig.MultipleEffects
                ? subsetManager.SelectedEffect
                : null;
        }

        protected override void summarizeActionResult(IEffectsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new EffectsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
