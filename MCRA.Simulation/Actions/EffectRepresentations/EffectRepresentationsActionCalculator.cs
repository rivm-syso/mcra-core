using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.EffectRepresentations {

    [ActionType(ActionType.EffectRepresentations)]
    public class EffectRepresentationsActionCalculator : ActionCalculatorBase<IEffectRepresentationsActionResult> {

        public EffectRepresentationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = _project.EffectSettings.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = _project.EffectSettings.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.EffectRepresentations][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;
            var allResponses = data.Responses.Values.ToHashSet();
            var selectedEffectRepresentations = subsetManager.AllEffectRepresentations
                .Where(r => relevantEffects.Contains(r.Key))
                .SelectMany(r => r)
                .Where(r => allResponses.Contains(r.Response))
                .ToList();
            data.FocalEffectRepresentations = selectedEffectRepresentations;
            data.AllEffectRepresentations = subsetManager.AllEffectRepresentations;
        }

        protected override void summarizeActionResult(IEffectRepresentationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(0);
            var summarizer = new EffectRepresentationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
