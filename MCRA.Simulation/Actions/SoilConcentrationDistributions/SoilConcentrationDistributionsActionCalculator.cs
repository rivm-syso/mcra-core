using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {

    [ActionType(ActionType.SoilConcentrationDistributions)]
    public class SoilConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<SoilConcentrationDistributionsActionResult>(project) {
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SoilConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var concentrationModelsBuilder = new SoilConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                subsetManager.AllSoilConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0,
                SystemUnits.DefaultSoilConcentrationUnit
            );

            data.SoilConcentrationModels = concentrationModels;
            data.SoilConcentrationDistributionUnit = SystemUnits.DefaultSoilConcentrationUnit;
        }

        protected override SoilConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            return compute(data, progressReport);
        }

        private static SoilConcentrationDistributionsActionResult compute(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new SoilConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                data.SoilConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new SoilConcentrationDistributionsActionResult() {
                SoilConcentrationModels = concentrationModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, SoilConcentrationDistributionsActionResult result) {
            data.SoilConcentrationModels = result.SoilConcentrationModels;
        }

        protected override void summarizeActionResult(
            SoilConcentrationDistributionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil concentration distributions", 0);
            var summarizer = new SoilConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override SoilConcentrationDistributionsActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            return compute(data, progressReport);
        }
    }
}
