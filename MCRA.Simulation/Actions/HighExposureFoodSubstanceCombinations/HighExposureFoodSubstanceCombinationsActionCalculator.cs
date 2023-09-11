using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {

    [ActionType(ActionType.HighExposureFoodSubstanceCombinations)]
    public class HighExposureFoodSubstanceCombinationsActionCalculator : ActionCalculatorBase<HighExposureFoodSubstanceCombinationsActionResult> {

        public HighExposureFoodSubstanceCombinationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.Effects].IsRequired = _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.Effects].IsVisible = _project.AssessmentSettings.Cumulative;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HighExposureFoodSubstanceCombinationsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override HighExposureFoodSubstanceCombinationsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Screening exposures", 0);

            // TODO: remove this assignment of dietary exposure unit
            // this data field is part of the dietary exposures module,
            // not of this module.
            data.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(
                data.ConsumptionUnit, 
                data.ConcentrationUnit, 
                data.BodyWeightUnit, 
                _project.SubsetSettings.IsPerPerson
            );

            // Create screening calculator factory based on settings.
            var settings = new ScreeningCalculatorFactorySettings(_project.ScreeningSettings, _project.AssessmentSettings);
            var screeningFactory = new ScreeningCalculatorFactory(
                settings,
                _project.SubsetSettings.IsPerPerson
            );

            // Compute screening results.
            var screeningCalculator = screeningFactory.Create();
            var screeningResult = screeningCalculator.Calculate(
                data.FoodConversionResults,
                data.ModelledFoodConsumerDays,
                data.SelectedFoodConsumptions,
                data.CompoundResidueCollections.Values,
                data.CorrectedRelativePotencyFactors,
                progressReport
            );
            var result = new HighExposureFoodSubstanceCombinationsActionResult() {
                ScreeningResult = screeningResult
            };
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(HighExposureFoodSubstanceCombinationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(0);
            var summarizer = new HighExposureFoodSubstanceCombinationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, HighExposureFoodSubstanceCombinationsActionResult result) {
            data.ScreeningResult = result.ScreeningResult;
        }
    }
}
