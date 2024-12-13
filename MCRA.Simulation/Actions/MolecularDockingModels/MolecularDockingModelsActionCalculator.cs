using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.MolecularDockingModels {

    [ActionType(ActionType.MolecularDockingModels)]
    public class MolecularDockingModelsActionCalculator : ActionCalculatorBase<IMolecularDockingModelsActionResult> {
        private MolecularDockingModelsModuleConfig ModuleConfig => (MolecularDockingModelsModuleConfig)_moduleSettings;


        public MolecularDockingModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = ModuleConfig.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = ModuleConfig.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.MolecularDockingModels][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.MolecularBindingEnergies][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var relevantEffects = data.RelevantEffects ??
                (ModuleConfig.MultipleEffects ? data.AllEffects : [data.SelectedEffect]);
            var models = subsetManager.AllMolecularDockingModels
                .Where(r => relevantEffects.Contains(r.Effect))
                .ToList();
            data.MolecularDockingModels = models;
        }

        protected override void summarizeActionResult(IMolecularDockingModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            if (data.MolecularDockingModels != null) {
                var summarizer = new MolecularDockingModelsSummarizer();
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }
    }
}
