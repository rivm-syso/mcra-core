using MCRA.Utils.ProgressReporting;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {

    [ActionType(ActionType.SingleValueDietaryExposures)]
    public class SingleValueDietaryExposuresActionCalculator : ActionCalculatorBase<SingleValueDietaryExposuresActionResult> {

        public SingleValueDietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            bool useUnitVariability = _project.AssessmentSettings.ExposureType == ExposureType.Acute && _project.UnitVariabilitySettings.UseUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsRequired = useUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsVisible = useUnitVariability;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = _project.ConcentrationModelSettings.IsProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = _project.ConcentrationModelSettings.IsProcessing;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsVisible = _project.AgriculturalUseSettings.UseOccurrenceFrequencies;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsRequired = _project.AgriculturalUseSettings.UseOccurrenceFrequencies;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new SingleValueDietaryExposuresSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueDietaryExposuresSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override SingleValueDietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var exposureUnit = TargetUnit.CreateSingleValueDietaryExposureUnit(
                data.SingleValueConsumptionIntakeUnit,
                data.ConcentrationUnit,
                BodyWeightUnit.kg,
                false
            );
            
            localProgress.Update("Calculating single value dietary exposures", 0);
            //Hit summarizer settings, is needed
            _ = _project.AssessmentSettings.ExposureType;
            var calculator = SingleValueDietaryExposureCalculatorFactory.Create(
                _project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod,
                _project.UnitVariabilitySettings.UseUnitVariability 
                    ? data.UnitVariabilityDictionary
                    : null,
                data.IestiSpecialCases,
                data.ProcessingFactorModels
            );

            var results = calculator.Compute(
                data.SelectedPopulation,
                data.ActiveSubstances,
                data.SingleValueConsumptionModels,
                data.ActiveSubstanceSingleValueConcentrations,
                data.OccurrenceFractions,
                data.SingleValueConsumptionIntakeUnit,
                data.ConcentrationUnit,
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
            var summarizer = new SingleValueDietaryExposuresSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
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
