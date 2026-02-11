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

namespace MCRA.Simulation.Actions.AirConcentrationDistributions {

    [ActionType(ActionType.AirConcentrationDistributions)]
    public class AirConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<AirConcentrationDistributionsActionResult>(project) {
        private AirConcentrationDistributionsModuleConfig ModuleConfig => (AirConcentrationDistributionsModuleConfig)_moduleSettings;
        protected override void verify() {

            _actionInputRequirements[ActionType.OutdoorAirConcentrations].IsRequired = false;
            _actionInputRequirements[ActionType.OutdoorAirConcentrations].IsVisible = true;
            _actionInputRequirements[ActionType.IndoorAirConcentrations].IsRequired = false;
            _actionInputRequirements[ActionType.IndoorAirConcentrations].IsVisible = true;
            _actionDataLinkRequirements[ScopingType.IndoorAirDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.OutdoorAirDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;

        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var concentrationModelsBuilder = new AirConcentrationModelBuilder();
            var indoorConcentrationModels = concentrationModelsBuilder.Create(
                subsetManager.AllIndoorAirConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0,
                SystemUnits.DefaultAirConcentrationUnit
            );
            var outdoorConcentrationModels = concentrationModelsBuilder.Create(
                subsetManager.AllOutdoorAirConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0,
                SystemUnits.DefaultAirConcentrationUnit
            );

            data.IndoorAirConcentrationModels = indoorConcentrationModels;
            data.OutdoorAirConcentrationModels = outdoorConcentrationModels;
            data.AirConcentrationDistributionUnit = SystemUnits.DefaultAirConcentrationUnit;
        }

        protected override AirConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new AirConcentrationModelBuilder();
            var indoorConcentrationModels = concentrationModelsBuilder.Create(
                data.IndoorAirConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var outdoorConcentrationModels = concentrationModelsBuilder.Create(
                data.OutdoorAirConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new AirConcentrationDistributionsActionResult() {
                IndoorAirConcentrationModels = indoorConcentrationModels,
                OutdoorAirConcentrationModels = outdoorConcentrationModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, AirConcentrationDistributionsActionResult result) {
            data.IndoorAirConcentrationModels = result.IndoorAirConcentrationModels;
            data.OutdoorAirConcentrationModels = result.OutdoorAirConcentrationModels;
        }

        protected override void summarizeActionResult(
            AirConcentrationDistributionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing Air concentration distributions", 0);
            var summarizer = new AirConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override AirConcentrationDistributionsActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new AirConcentrationModelBuilder();
            var indoorConcentrationModels = concentrationModelsBuilder.Create(
                data.IndoorAirConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var outdoorConcentrationModels = concentrationModelsBuilder.Create(
                data.OutdoorAirConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new AirConcentrationDistributionsActionResult() {
                IndoorAirConcentrationModels = indoorConcentrationModels,
                OutdoorAirConcentrationModels = outdoorConcentrationModels,
            };
            localProgress.Update(100);
            return result;
        }
    }
}
