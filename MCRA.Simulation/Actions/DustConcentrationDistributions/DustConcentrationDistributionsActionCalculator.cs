using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {

    [ActionType(ActionType.DustConcentrationDistributions)]
    public class DustConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<DustConcentrationDistributionsActionResult>(project) {
        private DustConcentrationDistributionsModuleConfig ModuleConfig => (DustConcentrationDistributionsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                subsetManager.AllDustConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0,
                SystemUnits.DefaultDustConcentrationUnit
            );

            data.DustConcentrationModels = concentrationModels;
            data.DustConcentrationDistributionUnit = SystemUnits.DefaultDustConcentrationUnit;
        }

        protected override DustConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                data.DustConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new DustConcentrationDistributionsActionResult() {
                DustConcentrationModels = concentrationModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, DustConcentrationDistributionsActionResult result) {
            data.DustConcentrationModels = result.DustConcentrationModels;
        }

        protected override void summarizeActionResult(
            DustConcentrationDistributionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust concentration distributions", 0);
            var summarizer = new DustConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override DustConcentrationDistributionsActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                data.DustConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new DustConcentrationDistributionsActionResult() {
                DustConcentrationModels = concentrationModels,
            };
            localProgress.Update(100);
            return result;
        }
    }
}
