using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    [ActionType(ActionType.SingleValueConcentrations)]
    public sealed class SingleValueConcentrationsActionCalculator : ActionCalculatorBase<SingleValueConcentrationsActionResult> {
        public SingleValueConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isCompute = _project.CalculationActionTypes?.Contains(ActionType.SingleValueConcentrations) ?? false;
            var useMrl = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = _project.ConcentrationModelSettings.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = _project.ConcentrationModelSettings.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.Concentrations].IsVisible = isCompute;
            _actionInputRequirements[ActionType.Concentrations].IsRequired = isCompute;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = isCompute;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = isCompute && useMrl;
            _actionDataLinkRequirements[ScopingType.ConcentrationSingleValues][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationSingleValues][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsVisible = _project.ConcentrationModelSettings.UseDeterministicConversionFactors;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsRequired = _project.ConcentrationModelSettings.UseDeterministicConversionFactors;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueConcentrationsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.MeasuredSubstances = subsetManager.AllConcentrationSingleValues.Select(r => r.Substance).ToHashSet();
            data.SingleValueConcentrations = subsetManager.AllConcentrationSingleValues;
            var builder = new SingleValueConcentrationsBuilder();
            data.MeasuredSubstanceSingleValueConcentrations = builder.Create(
                data.SingleValueConcentrations,
                data.ConcentrationUnit
            );
            if (_project.ConcentrationModelSettings.UseDeterministicConversionFactors
                && (data.DeterministicSubstanceConversionFactors?.Any() ?? false)) {
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
            var measuredSubstanceSingleValueConcentrations = calculator.Compute(
                data.AllFoods,
                data.ActiveSubstances,
                data.ActiveSubstanceSampleCollections,
                data.MaximumConcentrationLimits
            );

            IDictionary<(Food, Compound), SingleValueConcentrationModel> activeSubstanceSingleValueConcentrations;
            if (_project.ConcentrationModelSettings.UseDeterministicConversionFactors
                && (data.DeterministicSubstanceConversionFactors?.Any() ?? false)) {
                var conversionCalculator = new SingleValueConcentrationConversionCalculator();
                activeSubstanceSingleValueConcentrations = conversionCalculator.Compute(
                    data.ActiveSubstances,
                    measuredSubstanceSingleValueConcentrations,
                    data.DeterministicSubstanceConversionFactors
                );
            } else {
                activeSubstanceSingleValueConcentrations = measuredSubstanceSingleValueConcentrations;
            }

            var result = new SingleValueConcentrationsActionResult {
                ConcentrationUnit = data.ConcentrationUnit,
                MeasuredSubstanceSingleValueConcentrations = measuredSubstanceSingleValueConcentrations,
                ActiveSubstanceSingleValueConcentrations = activeSubstanceSingleValueConcentrations
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, SingleValueConcentrationsActionResult result) {
            data.MeasuredSubstanceSingleValueConcentrations = result.MeasuredSubstanceSingleValueConcentrations;
            data.ActiveSubstanceSingleValueConcentrations = result.ActiveSubstanceSingleValueConcentrations;
        }

        protected override void summarizeActionResult(SingleValueConcentrationsActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SingleValueConcentrationsSummarizer();
            summarizer.Summarize(_project, result, data, header, order);
            localProgress.Update(100);
        }
    }
}

