using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.AOPNetworks {

    [ActionType(ActionType.AOPNetworks)]
    public class AOPNetworksActionCalculator : ActionCalculatorBase<IAOPNetworksCalculationActionResult> {
        private AOPNetworksModuleConfig ModuleConfig => (AOPNetworksModuleConfig)_moduleSettings;

        public AOPNetworksActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.AdverseOutcomePathwayNetworks][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.EffectRelations][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new AOPNetworksSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize();
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AdverseOutcomePathwayNetwork = (!string.IsNullOrEmpty(ModuleConfig.CodeAopNetwork)
                && subsetManager.AllAdverseOutcomePathwayNetworks.TryGetValue(ModuleConfig.CodeAopNetwork, out var aopn))
                ? aopn : null;
            if (data.AdverseOutcomePathwayNetwork == null) {
                throw new Exception("No AOP network selected.");
            }
            if (ModuleConfig.RestrictAopByFocalUpstreamEffect) {
                var focalUpstreamEffect = data.AllEffects.FirstOrDefault(r => r.Code == ModuleConfig.CodeFocalUpstreamEffect);
                var subNetwork = data.AdverseOutcomePathwayNetwork.GetSubNetwork(data.SelectedEffect, focalUpstreamEffect);
                data.RelevantEffects = subNetwork.GetAllEffects();
            } else {
                var subNetwork = data.AdverseOutcomePathwayNetwork.GetSubNetwork(data.SelectedEffect ?? data.AdverseOutcomePathwayNetwork.AdverseOutcome, null);
                data.RelevantEffects = subNetwork.GetAllEffects();
            }
        }

        protected override void summarizeActionResult(IAOPNetworksCalculationActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            if (data.AdverseOutcomePathwayNetwork != null) {
                var summarizer = new AOPNetworksSummarizer();
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }
    }
}
