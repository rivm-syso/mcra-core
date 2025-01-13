using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {

    [ActionType(ActionType.SingleValueNonDietaryExposures)]
    public class SingleValueNonDietaryExposuresActionCalculator : ActionCalculatorBase<SingleValueNonDietaryExposuresActionResult> {
        private SingleValueNonDietaryExposuresModuleConfig ModuleConfig => (SingleValueNonDietaryExposuresModuleConfig)_moduleSettings;

        public SingleValueNonDietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ExposureEstimates][ScopingType.ExposureDeterminantCombinations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ExposureEstimates][ScopingType.Compounds].AlertTypeMissingData = AlertType.Error;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueNonDietaryExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override SingleValueNonDietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var codeConfig = ModuleConfig.CodeConfiguration;
            var calculator = new OpexSingleValueNonDietaryExposureCalculator();
            var exposures = calculator.Compute(data.AllCompounds, codeConfig);

            var result = new SingleValueNonDietaryExposuresActionResult {
                SingleValueNonDietaryExposureScenarios = exposures.SingleValueNonDietaryExposureScenarios,
                SingleValueNonDietaryExposureDeterminantCombinations = exposures.SingleValueNonDietaryExposureDeterminantCombinations,
                SingleValueNonDietaryExposureEstimates = exposures.SingleValueNonDietaryExposureEstimates,
            };

            localProgress.Update(100);
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            data.SingleValueNonDietaryExposureEstimates = subsetManager.AllSingleValueNonDietaryExposures;
            data.SingleValueNonDietaryExposureScenarios = subsetManager.AllSingleValueNonDietaryExposureScenarios;
            data.SingleValueNonDietaryExposureDeterminantCombinations = subsetManager.AllSingleValueNonDietaryExposureDeterminantCombinations;

            localProgress.Update(100);
        }

        protected override void summarizeActionResult(SingleValueNonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new SingleValueNonDietaryExposuresSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, SingleValueNonDietaryExposuresActionResult result) {
            data.SingleValueNonDietaryExposureScenarios = result.SingleValueNonDietaryExposureScenarios;
            data.SingleValueNonDietaryExposureDeterminantCombinations = result.SingleValueNonDietaryExposureDeterminantCombinations;
            data.SingleValueNonDietaryExposureEstimates = result.SingleValueNonDietaryExposureEstimates;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, SingleValueNonDietaryExposuresActionResult result) {
            var outputWriter = new SingleValueNonDietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }
    }
}
