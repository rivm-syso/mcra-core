using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {

    [ActionType(ActionType.SingleValueConsumptions)]
    public sealed class SingleValueConsumptionsActionCalculator : ActionCalculatorBase<SingleValueConsumptionsActionResult> {
        private SingleValueConsumptionsModuleConfig ModuleConfig => (SingleValueConsumptionsModuleConfig)_moduleSettings;

        public SingleValueConsumptionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.ConsumptionsByModelledFood].IsVisible = _isCompute;
            _actionInputRequirements[ActionType.ConsumptionsByModelledFood].IsRequired = _isCompute;
            _actionDataLinkRequirements[ScopingType.PopulationConsumptionSingleValues][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueConsumptionSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.FoodConsumptionSingleValues = subsetManager.AllPopulationConsumptionSingleValues;
            var singleValueConsumptionsCollectionBuilder = new SingleValueConsumptionsCollectionBuilder();
            data.SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnitConverter.FromConsumptionUnit(data.ConsumptionUnit);
            data.SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg;
            data.SingleValueConsumptionModels = singleValueConsumptionsCollectionBuilder.Create(
                data.SelectedPopulation,
                data.FoodConsumptionSingleValues,
                data.SingleValueConsumptionIntakeUnit,
                data.SingleValueConsumptionBodyWeightUnit
            );
        }

        protected override SingleValueConsumptionsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new SingleValueConsumptionsCalculatorSettings(ModuleConfig);
            var consumptionSingleValueCalculator = new SingleValueConsumptionsCalculator(settings);
            var intakeUnit = ConsumptionIntakeUnitConverter.FromConsumptionUnit(data.ConsumptionUnit);
            var singleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg;
            var singleValueConsumptionsByModelledFood = consumptionSingleValueCalculator.Compute(
                data.ModelledFoodConsumerDays,
                data.ModelledFoodConsumers,
                data.ConsumptionsByModelledFood,
                data.ConsumptionUnit,
                data.BodyWeightUnit,
                intakeUnit,
                singleValueConsumptionBodyWeightUnit
            );
            var result = new SingleValueConsumptionsActionResult() {
                IntakeUnit = intakeUnit,
                SingleValueConsumptionsBodyWeightUnit = singleValueConsumptionBodyWeightUnit,
                SingleValueConsumptionsByModelledFood = singleValueConsumptionsByModelledFood,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, SingleValueConsumptionsActionResult result) {
            data.SingleValueConsumptionIntakeUnit = result.IntakeUnit;
            data.SingleValueConsumptionBodyWeightUnit = result.SingleValueConsumptionsBodyWeightUnit;
            data.SingleValueConsumptionModels = result.SingleValueConsumptionsByModelledFood;
        }

        protected override void summarizeActionResult(SingleValueConsumptionsActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SingleValueConsumptionsSummarizer();
            summarizer.Summarize(_actionSettings, result, data, header, order);
            localProgress.Update(100);
        }
    }
}
