using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
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
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var exposureResponseFunctions = subsetManager.AllExposureResponseFunctions;
            data.ExposureResponseFunctionModels = exposureResponseFunctions
                .Select(r => new ExposureResponseFunctionModel(r))
                .Cast<IExposureResponseFunctionModel>()
                .ToList();
        }

        protected override void summarizeActionResult(IExposureResponseFunctionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureResponseFunctionsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.ExposureResponseFunctionModels != null && factorialSet.Contains(UncertaintySource.ExposureResponseFunctions)) {
                localProgress.Update("Resampling exposure response functions.");
                if (data.ExposureResponseFunctionModels?.Count > 0) {
                    var random = uncertaintySourceGenerators[UncertaintySource.ExposureResponseFunctions];
                    foreach (var model in data.ExposureResponseFunctionModels) {
                        model.ResampleModelParameters(random);
                    }
                }
            }
            localProgress.Update(100);
        }
    }
}
