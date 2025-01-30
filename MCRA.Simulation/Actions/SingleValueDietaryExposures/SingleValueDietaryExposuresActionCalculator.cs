using MCRA.Utils.ProgressReporting;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {

    [ActionType(ActionType.SingleValueDietaryExposures)]
    public class SingleValueDietaryExposuresActionCalculator : ActionCalculatorBase<SingleValueDietaryExposuresActionResult> {
        private SingleValueDietaryExposuresModuleConfig ModuleConfig => (SingleValueDietaryExposuresModuleConfig)_moduleSettings;

        public SingleValueDietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            bool useUnitVariability = ModuleConfig.ExposureType == ExposureType.Acute && ModuleConfig.UseUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsRequired = useUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsVisible = useUnitVariability;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = ModuleConfig.IsProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = ModuleConfig.IsProcessing;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsVisible = ModuleConfig.UseOccurrenceFrequencies;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsRequired = ModuleConfig.UseOccurrenceFrequencies;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new SingleValueDietaryExposuresSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueDietaryExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override SingleValueDietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var exposureUnit = TargetUnit.CreateSingleValueDietaryExposureUnit(
                data.SingleValueConsumptionIntakeUnit,
                data.SingleValueConcentrationUnit,
                BodyWeightUnit.kg,
                false
            );

            // Processing factor provider
            var processingFactorProvider = ModuleConfig.IsProcessing
                ? new ProcessingFactorProvider(
                    data.ProcessingFactorModels,
                    false,
                    defaultMissingProcessingFactor: 1D
                ) : null;

            localProgress.Update("Calculating single value dietary exposures", 0);
            var calculator = SingleValueDietaryExposureCalculatorFactory.Create(
                ModuleConfig.SingleValueDietaryExposureCalculationMethod,
                ModuleConfig.UseUnitVariability
                    ? data.UnitVariabilityDictionary
                    : null,
                data.IestiSpecialCases,
                processingFactorProvider
            );

            var results = calculator.Compute(
                data.SelectedPopulation,
                data.ActiveSubstances,
                data.SingleValueConsumptionModels,
                data.ActiveSubstanceSingleValueConcentrations,
                data.OccurrenceFractions,
                data.SingleValueConsumptionIntakeUnit,
                data.SingleValueConcentrationUnit,
                data.SingleValueConsumptionBodyWeightUnit,
                exposureUnit
            );

            var result = new SingleValueDietaryExposuresActionResult {
                Exposures = results,
                ExposureUnit = exposureUnit
            };

            localProgress.Update(100);

            return result;
        }

        protected override void summarizeActionResult(SingleValueDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new SingleValueDietaryExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, SingleValueDietaryExposuresActionResult result) {
            data.SingleValueDietaryExposureUnit = result.ExposureUnit;
            data.SingleValueDietaryExposureResults = result.Exposures;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, SingleValueDietaryExposuresActionResult result) {
            var outputWriter = new SingleValueDietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }
    }
}
