using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ProcessingFactors {

    [ActionType(ActionType.ProcessingFactors)]
    public class ProcessingFactorsActionCalculator : ActionCalculatorBase<IProcessingFactorsActionResult> {
        private ProcessingFactorsModuleConfig ModuleConfig => (ProcessingFactorsModuleConfig)_moduleSettings;

        public ProcessingFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.ProcessingTypes].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (ModuleConfig.ResampleProcessingFactors) {
                result.Add(UncertaintySource.Processing);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ProcessingFactorsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            data.ProcessingFactors = subsetManager.AllProcessingFactors;
            var processingFactorModelsBuilder = new ProcessingFactorModelCollectionBuilder();
            data.ProcessingFactorModels = processingFactorModelsBuilder
                .Create(
                    data.ProcessingFactors,
                    data.ActiveSubstances,
                    ModuleConfig.IsDistribution,
                    ModuleConfig.AllowHigherThanOne
                );
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IProcessingFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ProcessingFactorsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.Processing) && data.ProcessingFactorModels != null) {
                localProgress.Update("Resampling processing factors");
                var processingFactorCalculator = new ProcessingFactorModelCollectionBuilder();
                processingFactorCalculator.Resample(
                    uncertaintySourceGenerators[UncertaintySource.Processing],
                    data.ProcessingFactorModels
                );
            }
            localProgress.Update(100);
        }
    }
}
