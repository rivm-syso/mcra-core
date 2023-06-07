using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ProcessingFactors {

    [ActionType(ActionType.ProcessingFactors)]
    public class ProcessingFactorsActionCalculator : ActionCalculatorBase<IProcessingFactorsActionResult> {

        public ProcessingFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.ProcessingTypes].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ProcessingFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (_project.UncertaintyAnalysisSettings.ReSampleProcessingFactors) {
                result.Add(UncertaintySource.Processing);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ProcessingFactorsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100); 
            data.ProcessingFactors = subsetManager.AllProcessingFactors;
            var settings = new ProcessingFactorModelCollectionBuilderSettings(_project.ConcentrationModelSettings);
            var processingFactorModelsBuilder = new ProcessingFactorModelCollectionBuilder(settings);
            data.ProcessingFactorModels = processingFactorModelsBuilder.Create(
                data.ProcessingFactors,
                data.ActiveSubstances);
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IProcessingFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ProcessingFactorsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new ProcessingFactorModelCollectionBuilderSettings(_project.ConcentrationModelSettings);
            if (factorialSet.Contains(UncertaintySource.Processing) && data.ProcessingFactorModels != null) {
                localProgress.Update("Resampling processing factors");
                var processingFactorCalculator = new ProcessingFactorModelCollectionBuilder(settings);
                processingFactorCalculator.Resample(uncertaintySourceGenerators[UncertaintySource.Processing], data.ProcessingFactorModels);
            }
            localProgress.Update(100);
        }
    }
}
