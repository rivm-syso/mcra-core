using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Effects {

    [ActionType(ActionType.Effects)]
    public class EffectsActionCalculator : ActionCalculatorBase<IEffectsActionResult> {

        public EffectsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.Effects].AllowCodesInScopeNotInSource = true;
            if (!_project.EffectSettings.MultipleEffects
                && !_project.EffectSettings.IncludeAopNetwork
                && (!_project.LoopScopingTypes?.Contains(ScopingType.Effects) ?? true)
            ) {
                _actionDataSelectionRequirements[ScopingType.Effects].MaxSelectionCount = 1;
            }
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (_project.EffectSettings.MultipleEffects) {
                return true;
            } else if (_project.EffectSettings.IncludeAopNetwork) {
                if (!string.IsNullOrEmpty(_project.EffectSettings.CodeFocalEffect)) {
                    return linkManager.GetCodesInScope(ScopingType.Effects).Contains(_project.EffectSettings.CodeFocalEffect);
                }
                return false;
            } else {
                return linkManager.GetCodesInScope(ScopingType.Effects).Count == 1;
            }
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new EffectsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AllEffects = subsetManager.AllEffects.Values;
            data.SelectedEffect = !_project.EffectSettings.MultipleEffects
                ? subsetManager.SelectedEffect
                : null;
        }

        protected override void summarizeActionResult(IEffectsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new EffectsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
