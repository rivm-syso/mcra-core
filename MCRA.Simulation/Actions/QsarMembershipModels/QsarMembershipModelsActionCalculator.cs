using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.QsarMembershipModels {

    [ActionType(ActionType.QsarMembershipModels)]
    public class QsarMembershipModelsActionCalculator : ActionCalculatorBase<IQsarMembershipModelsActionResult> {
        private QsarMembershipModelsModuleConfig ModuleConfig => (QsarMembershipModelsModuleConfig)_moduleSettings;

        public QsarMembershipModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = ModuleConfig.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = ModuleConfig.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.QsarMembershipModels][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.QsarMembershipScores][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var relevantEffects = data.RelevantEffects ??
                (ModuleConfig.MultipleEffects ? data.AllEffects : new List<Effect>() { data.SelectedEffect });
            var models = subsetManager.AllQsarMembershipModels
                .Where(r => relevantEffects.Contains(r.Effect))
                .ToList();
            data.QsarMembershipModels = models;
        }

        protected override void summarizeActionResult(IQsarMembershipModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            if (data.QsarMembershipModels != null) {
                var summarizer = new QsarMembershipModelsSummarizer();
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }
    }
}
