using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
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
            var isMonitoringConcentrations = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration
                && !ModuleConfig.UsePointEstimates;
            var isHbmSingleValueExposures = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration
                && ModuleConfig.UsePointEstimates;
            var isComputeFromModelledExposures = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsVisible = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsRequired = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HbmSingleValueExposures].IsRequired = isHbmSingleValueExposures;
            _actionInputRequirements[ActionType.HbmSingleValueExposures].IsVisible = isHbmSingleValueExposures;
            var isCumulative = ModuleConfig.MultipleSubstances && ModuleConfig.Cumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative;
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
            if (_project.ActionSettings.ExposureType != ExposureType.Chronic) {
                throw new Exception("Environmental burden of disease actions are only allowed for chronic exposure");
            }

            if (ModuleConfig.BodApproach == BodApproach.BottomUp &&
                data.SelectedPopulation.Size == 0D) {
                throw new Exception("Population size is required for bottom-up burden of disease computations.");
            }

            // Get burdens of disease
            var burdensOfDisease = data.BurdensOfDisease;

            if (data.BodIndicatorConversions != null) {
                // Get derived burdens of disease from BoD indicator conversions
                var bodIndicatorConversionsCalculator = new BoDConversionsCalculator();
                var derivedBodIndicators = bodIndicatorConversionsCalculator
                    .GetDerivedBurdensOfDisease(
                        data.BurdensOfDisease,
                        data.BodIndicatorConversions
                    );
                burdensOfDisease = [.. burdensOfDisease.Union(derivedBodIndicators)];
            }

            // Only keep BoDs for selected indicators
            burdensOfDisease = [.. burdensOfDisease.Where(r => ModuleConfig.BodIndicators.Contains(r.BodIndicator))];

            // Compute exposure response
            var erCalculator = new ExposureResponseCalculator(
                ModuleConfig.ExposureGroupingMethod,
                ModuleConfig.BinBoundaries,
                ModuleConfig.WithinBinExposureRepresentationMethod
            );

            // Get exposure response function models (filter by reference substance if cumulative)
            var exposureResponseFunctionModels = ModuleConfig.MultipleSubstances && ModuleConfig.Cumulative
                ? data.ExposureResponseFunctionModels
                    .Where(r => r.ExposureResponseFunction.Substance == data.ReferenceSubstance)
                    .ToList()
                : data.ExposureResponseFunctionModels;

            // Compute exposure response results
            var exposureResponseResults = ModuleConfig.UsePointEstimates
                ? erCalculator.ComputeFromHbmSingleValueExposures(
                    data.HbmSingleValueExposureSets,
                    exposureResponseFunctionModels
                )
                : erCalculator.ComputeFromTargetIndividualExposures(
                    getExposures(data, ModuleConfig.MultipleSubstances && ModuleConfig.Cumulative),
                    exposureResponseFunctionModels
                );

            // Create EBD calculator and compute
            var ebdCalculator = new EnvironmentalBurdenOfDiseaseCalculator(
                ModuleConfig.BodApproach,
                ModuleConfig.EbdStandardisation
            );
            var environmentalBurdenOfDiseases = ebdCalculator.Compute(
                burdensOfDisease,
                data.SelectedPopulation,
                exposureResponseResults
            );

            if (environmentalBurdenOfDiseases.Count == 0) {
                throw new Exception($"No burden of diseases are found in table or through indicator conversion for selected Bod indicators.");
            }

            var result = new EnvironmentalBurdenOfDiseaseActionResult {
                EnvironmentalBurdenOfDiseases = environmentalBurdenOfDiseases,
                ExposureResponseResults = exposureResponseResults,
            };

            localProgress.Update(100);
            return result;
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
            ActionData data,
            bool cumulative
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

            // If cumulative, then compute cumulative exposures
            if (cumulative) {
                result = CumulativeTargetExposuresCalculator
                    .ComputeCumulativeExposures(
                        result,
                        data.ReferenceSubstance,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
            }
            return result;
        }
    }
}
