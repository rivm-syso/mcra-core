using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {

    [ActionType(ActionType.EnvironmentalBurdenOfDisease)]
    public class EnvironmentalBurdenOfDiseaseActionCalculator : ActionCalculatorBase<EnvironmentalBurdenOfDiseaseActionResult> {
        private EnvironmentalBurdenOfDiseaseModuleConfig ModuleConfig => (EnvironmentalBurdenOfDiseaseModuleConfig)_moduleSettings;

        public EnvironmentalBurdenOfDiseaseActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isTargetLevelExternal = ModuleConfig.TargetDoseLevelType == TargetLevelType.External;
            var isMonitoringConcentrations = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration;
            var isComputeFromModelledExposures = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsVisible = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsRequired = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new EnvironmentalBurdenOfDiseaseSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override EnvironmentalBurdenOfDiseaseActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress);
        }

        protected override EnvironmentalBurdenOfDiseaseActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress, factorialSet, uncertaintySourceGenerators);
        }

        private EnvironmentalBurdenOfDiseaseActionResult compute(
            ActionData data,
            ProgressState localProgress,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            if (_project.ActionSettings.ExposureType == ExposureType.Chronic) {

                if (ModuleConfig.BodApproach == BodApproach.BottomUp &&
                    data.SelectedPopulation.Size == 0D) {
                    throw new Exception("Population size is required for bottom-up burden of disease computations.");
                }

                var percentileIntervals = generatePercentileIntervals(ModuleConfig.BinBoundaries);

                var result = new EnvironmentalBurdenOfDiseaseActionResult();
                var environmentalBurdenOfDiseases = new List<EnvironmentalBurdenOfDiseaseResultRecord>();

                var exposureResponseFunctions = data.ExposureResponseFunctionModels
                    .Select(r => r.ExposureResponseFunction)
                    .ToList();
                var burdenOfDiseaseIndicators = data.BaselineBodIndicators
                    .Where(r => ModuleConfig.BodIndicators.Contains(r.BodIndicator))
                    .ToList();

                // Get exposures per target
                var exposuresCollections = getExposures(data);

                foreach (var exposureResponseFunctionModel in data.ExposureResponseFunctionModels) {
                    var erf = exposureResponseFunctionModel.ExposureResponseFunction;

                    var target = erf.ExposureTarget;

                    // Get exposures for target
                    if (!exposuresCollections.TryGetValue(target, out var targetExposures)) {
                        var msg = $"Failed to compute effects for exposure response function {erf.Code}: missing estimates for target {target.GetDisplayName()}.";
                        throw new Exception(msg);
                    }
                    (var exposures, var exposureUnit) = (targetExposures.Exposures, targetExposures.Unit);

                    var exposureResponseCalculator = new ExposureResponseCalculator(exposureResponseFunctionModel);
                    var exposureResponseResults = exposureResponseCalculator
                        .Compute(
                            exposures,
                            exposureUnit,
                            percentileIntervals,
                            ModuleConfig.ExposureGroupingMethod
                        );

                    foreach (var burdenOfDiseaseIndicator in burdenOfDiseaseIndicators) {
                        var totalBurdenOfDisease = burdenOfDiseaseIndicator.Value;
                        var ebdCalculator = new EnvironmentalBurdenOfDiseaseCalculator(
                            exposureResponseResults,
                            burdenOfDiseaseIndicator,
                            data.SelectedPopulation,
                            ModuleConfig.BodApproach
                        );
                        var resultRecords = ebdCalculator.Compute();
                        foreach (var resultRecord in resultRecords) {
                            environmentalBurdenOfDiseases.Add(resultRecord);
                        }
                    }
                }

                result.EnvironmentalBurdenOfDiseases = environmentalBurdenOfDiseases;
                localProgress.Update(100);
                return result;
            } else {
                throw new Exception("Environmental burden of disease actions are only allowed for chronic exposure");
            }
        }

        protected override void summarizeActionResult(EnvironmentalBurdenOfDiseaseActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new EnvironmentalBurdenOfDiseaseSummarizer(ModuleConfig);
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, EnvironmentalBurdenOfDiseaseActionResult result) {
            data.EnvironmentalBurdenOfDiseases = result.EnvironmentalBurdenOfDiseases;
        }
        protected override void summarizeActionResultUncertain(
            UncertaintyFactorialSet factorialSet,
            EnvironmentalBurdenOfDiseaseActionResult actionResult,
            ActionData data,
            SectionHeader header,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new EnvironmentalBurdenOfDiseaseSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, EnvironmentalBurdenOfDiseaseActionResult result) {
            updateSimulationData(data, result);
        }

        private Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> getExposures(
            ActionData data
        ) {
            var result = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure>, TargetUnit)>();
            if (ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration) {
                if (ModuleConfig.TargetDoseLevelType == TargetLevelType.External) {
                    // From dietary
                    var dietaryIndividualTargetExposures = data.DietaryIndividualDayIntakes
                        .AsParallel()
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => new DietaryIndividualTargetExposureWrapper([.. c], data.DietaryExposureUnit.ExposureUnit))
                        .OrderBy(r => r.SimulatedIndividual.Id)
                        .ToList();
                    result.Add(
                        ExposureTarget.DietaryExposureTarget,
                        (dietaryIndividualTargetExposures.Cast<ITargetIndividualExposure>().ToList(), data.DietaryExposureUnit)
                    );
                } else {
                    // From aggregate/internal exposures
                    var internalTargetExposures = data.AggregateIndividualExposures
                        .AsParallel()
                        .Select(c => new AggregateIndividualTargetExposureWrapper(c, data.TargetExposureUnit))
                        .OrderBy(r => r.SimulatedIndividual.Id)
                        .ToList();
                    result.Add(
                        data.TargetExposureUnit.Target,
                        (internalTargetExposures.Cast<ITargetIndividualExposure>().ToList(), data.TargetExposureUnit)
                    );
                }
            } else {
                // From HBM
                result = data.HbmIndividualCollections
                    .ToDictionary(
                        r => r.TargetUnit.Target,
                        r => (r.HbmIndividualConcentrations.Cast<ITargetIndividualExposure>().ToList(), r.TargetUnit)
                    );
            }
            return result;
        }

        private List<PercentileInterval> generatePercentileIntervals(List<double> binBoundaries) {
            binBoundaries = [.. binBoundaries.Order()];

            var lowerBinBoudaries = new List<double>(binBoundaries);
            lowerBinBoudaries.Insert(0, 0D);
            var upperBinBoudaries = new List<double>(binBoundaries) {
                    100D
                };

            var percentileIntervals = lowerBinBoudaries
                .Zip(upperBinBoudaries)
                .Select(r => new PercentileInterval(r.First, r.Second))
                .ToList();
            return percentileIntervals;
        }
    }
}
