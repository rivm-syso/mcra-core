using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    [ActionType(ActionType.SingleValueConcentrations)]
    public sealed class SingleValueConcentrationsActionCalculator : ActionCalculatorBase<SingleValueConcentrationsActionResult> {
        private SingleValueConcentrationsModuleConfig ModuleConfig => (SingleValueConcentrationsModuleConfig)_moduleSettings;

        public SingleValueConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var useMrl = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = ModuleConfig.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = ModuleConfig.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.Concentrations].IsVisible = _moduleSettings.IsCompute;
            _actionInputRequirements[ActionType.Concentrations].IsRequired = _moduleSettings.IsCompute;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = _moduleSettings.IsCompute;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = _moduleSettings.IsCompute && useMrl;
            _actionDataLinkRequirements[ScopingType.ConcentrationSingleValues][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationSingleValues][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsVisible = ModuleConfig.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsRequired = ModuleConfig.UseDeterministicConversionFactors;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueConcentrationsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.SingleValueConcentrations = subsetManager.AllConcentrationSingleValues;

            //Set single value concentration unit
            data.SingleValueConcentrationUnit = data.SingleValueConcentrations.Count == 1
                ? data.SingleValueConcentrations.First().ConcentrationUnit
                : SystemUnits.DefaultSingleValueConcentrationUnit;

            var builder = new SingleValueConcentrationsBuilder();
            data.MeasuredSubstanceSingleValueConcentrations = builder.Create(
                data.SingleValueConcentrations,
                data.SingleValueConcentrationUnit
            );
            if (ModuleConfig.UseDeterministicConversionFactors
                && (data.DeterministicSubstanceConversionFactors?.Count > 0)) {
                var conversionCalculator = new SingleValueConcentrationConversionCalculator();
                data.ActiveSubstanceSingleValueConcentrations = conversionCalculator.Compute(
                    data.ActiveSubstances,
                    data.MeasuredSubstanceSingleValueConcentrations,
                    data.DeterministicSubstanceConversionFactors
                );
            } else {
                data.ActiveSubstanceSingleValueConcentrations = data.MeasuredSubstanceSingleValueConcentrations;
            }
        }

        protected override SingleValueConcentrationsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var calculator = new SingleValueConcentrationsCalculator();
            var measuredSubstanceSingleValueConcentrations = calculator
                .Compute(
                    data.AllFoods,
                    data.ActiveSubstances,
                    data.ActiveSubstanceSampleCollections?.Values,
                    data.MaximumConcentrationLimits
                );

            IDictionary<(Food, Compound), SingleValueConcentrationModel> activeSubstanceSingleValueConcentrations;
            if (ModuleConfig.UseDeterministicConversionFactors
                && (data.DeterministicSubstanceConversionFactors?.Count > 0)) {
                var conversionCalculator = new SingleValueConcentrationConversionCalculator();
                activeSubstanceSingleValueConcentrations = conversionCalculator.Compute(
                    data.ActiveSubstances,
                    measuredSubstanceSingleValueConcentrations,
                    data.DeterministicSubstanceConversionFactors
                );
            } else {
                activeSubstanceSingleValueConcentrations = measuredSubstanceSingleValueConcentrations;
            }
            var singleValueConcentrationUnit = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First().ConcentrationUnit
                : ConcentrationUnit.mgPerKg;

            var result = new SingleValueConcentrationsActionResult {
                SingleValueConcentrationUnit = singleValueConcentrationUnit,
                MeasuredSubstanceSingleValueConcentrations = measuredSubstanceSingleValueConcentrations,
                ActiveSubstanceSingleValueConcentrations = activeSubstanceSingleValueConcentrations
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, SingleValueConcentrationsActionResult result) {
            data.MeasuredSubstanceSingleValueConcentrations = result.MeasuredSubstanceSingleValueConcentrations;
            data.ActiveSubstanceSingleValueConcentrations = result.ActiveSubstanceSingleValueConcentrations;
            data.SingleValueConcentrationUnit = result.SingleValueConcentrationUnit;
        }

        protected override void summarizeActionResult(SingleValueConcentrationsActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SingleValueConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, result, data, header, order);
            localProgress.Update(100);
        }
    }
}

