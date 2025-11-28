using CommandLine;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {

    [ActionType(ActionType.ExposureResponseFunctions)]
    public class ExposureResponseFunctionsActionCalculator : ActionCalculatorBase<IExposureResponseFunctionsActionResult> {

        private ExposureResponseFunctionsModuleConfig ModuleConfig => (ExposureResponseFunctionsModuleConfig)_moduleSettings;

        public ExposureResponseFunctionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ExposureResponseFunctions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ExposureResponseFunctions][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ErfSubgroups][ScopingType.ExposureResponseFunctions].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleExposureResponseFunctions) {
                result.Add(UncertaintySource.ExposureResponseFunctions);
            }
            if (ModuleConfig.ResampleCounterFactualValues) {
                result.Add(UncertaintySource.CounterFactualValues);
            }
            return result;
        }

        /// <summary>
        /// Load and create distribution voor exposure response functions, only when the response type is not a function.
        /// A function is dependent on the exposure level, which is unknown yet.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subsetManager"></param>
        /// <param name="progressState"></param>
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var exposureResponseFunctions = subsetManager.AllExposureResponseFunctions;
            data.ExposureResponseFunctionModels = [.. exposureResponseFunctions
                .Select(ExposureResponseModelBuilder.Create)
                .Cast<IExposureResponseModel>()];
        }

        protected override void summarizeActionResult(IExposureResponseFunctionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureResponseFunctionsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        /// <summary>
        /// Load and resample distribution voor exposure response functions, only when the response type is not a function.
        /// A function is dependent on the exposure level, which is unknown yet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="factorialSet"></param>
        /// <param name="uncertaintySourceGenerators"></param>
        /// <param name="progressReport"></param>
        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.ExposureResponseFunctionModels != null && data.ExposureResponseFunctionModels?.Count > 0) {
                foreach (var model in data.ExposureResponseFunctionModels) {
                    if (factorialSet.Contains(UncertaintySource.CounterFactualValues)) {
                        localProgress.Update("Resampling counter factual values.");
                        var random = uncertaintySourceGenerators[UncertaintySource.CounterFactualValues];
                        model.ResampleCounterFactualValue(random);
                    }
                    if (factorialSet.Contains(UncertaintySource.ExposureResponseFunctions)) {
                        localProgress.Update("Resampling exposure response functions.");
                        var random = uncertaintySourceGenerators[UncertaintySource.ExposureResponseFunctions];
                        model.ResampleExposureResponseFunction(random);
                    }
                }
            }
            localProgress.Update(100);
        }
    }
}
