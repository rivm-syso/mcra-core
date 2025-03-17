using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {

    [ActionType(ActionType.EnvironmentalBurdenOfDisease)]
    public class EnvironmentalBurdenOfDiseaseActionCalculator : ActionCalculatorBase<EnvironmentalBurdenOfDiseaseActionResult> {
        private EnvironmentalBurdenOfDiseaseModuleConfig ModuleConfig => (EnvironmentalBurdenOfDiseaseModuleConfig)_moduleSettings;

        public EnvironmentalBurdenOfDiseaseActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMonitoringConcentrations = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration;
            var isComputeFromModelledExposures = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = isComputeFromModelledExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = isComputeFromModelledExposures;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new EnvironmentalBurdenOfDiseaseSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override EnvironmentalBurdenOfDiseaseActionResult run(ActionData data, CompositeProgressState progressReport) {
            if (_project.ActionSettings.ExposureType == ExposureType.Chronic) {

                if (ModuleConfig.ExposureGroupingMethod == ExposureGroupingMethod.ErfDefinedBins) {
                    throw new NotImplementedException();
                }

                var lowerBinBoudaries = new List<double>(ModuleConfig.BinBoundaries);
                lowerBinBoudaries.Insert(0, 0D);
                var upperBinBoudaries = new List<double>(ModuleConfig.BinBoundaries) {
                    100D
                };

                var percentileIntervals = lowerBinBoudaries
                    .Zip(upperBinBoudaries)
                    .Select(r =>
                        new PercentileInterval(
                            r.First,
                            r.Second)
                        )
                    .ToList();

                var result = new EnvironmentalBurdenOfDiseaseActionResult();
                var environmentalBurdenOfDiseases = new List<EnvironmentalBurdenOfDiseaseResultRecord>();

                var exposureEffectFunctions = data.ExposureEffectFunctions
                    .Where(r => r.Substance == data.ActiveSubstances.First() &&
                        r.Effect == data.SelectedEffect);

                // Get exposures per target
                var exposuresCollections = getExposures(data);

                foreach (var exposureEffectFunction in exposureEffectFunctions) {
                    var target = exposureEffectFunction.ExposureTarget;

                    // Get exposures for target
                    var (exposures, exposureUnit) = exposuresCollections[target];

                    if (exposures == null) {
                        var msg = $"Failed to compute effects for exposure effect function {exposureEffectFunction.Code}: missing estimates available for matrix {target.GetDisplayName()}.";
                        throw new Exception(msg);
                    }

                    var exposureEffectCalculator = new ExposureEffectCalculator(exposureEffectFunction);
                    var exposureEffectResults = exposureEffectCalculator.Compute(
                        exposures,
                        exposureUnit,
                        percentileIntervals,
                        progressReport.NewCompositeState(100)
                    );

                    var burdenOfDiseaseIndicators = data.BaselineBodIndicators
                        .Where(r => r.Effect == data.SelectedEffect
                            && ModuleConfig.BodIndicators.Contains(r.BodIndicator));

                    foreach (var burdenOfDiseaseIndicator in burdenOfDiseaseIndicators) {
                        var totalBurdenOfDisease = burdenOfDiseaseIndicator
                        .Value;

                        var ebdCalculator = new EnvironmentalBurdenOfDiseaseCalculator(
                            exposureEffectResults,
                            burdenOfDiseaseIndicator
                        );
                        var resultRecords = ebdCalculator.Compute();

                        foreach (var resultRecord in resultRecords) {
                            environmentalBurdenOfDiseases.Add(resultRecord);
                        }
                    }
                }

                result.EnvironmentalBurdenOfDiseases = environmentalBurdenOfDiseases;
                //result.ExposureEffects = exposureEffectResults;

                return result;
            } else {
                throw new Exception("Environmental burden of disease actions are only allowed for chronic exposure");
            }
        }

        protected override void summarizeActionResult(EnvironmentalBurdenOfDiseaseActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new EnvironmentalBurdenOfDiseaseSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, EnvironmentalBurdenOfDiseaseActionResult result) {
            data.EnvironmentalBurdenOfDiseases = result.EnvironmentalBurdenOfDiseases;
            data.ExposureEffects = result.ExposureEffects;
        }

        private Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> getExposures(
            ActionData data
        ) {
            var result = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure>, TargetUnit)>();
            if (ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration) {
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
                // From HBM
                result = data.HbmIndividualCollections
                    .ToDictionary(
                        r => r.TargetUnit.Target,
                        r => (r.HbmIndividualConcentrations.Cast<ITargetIndividualExposure>().ToList(), r.TargetUnit)
                    );
            }
            return result;
        }
    }
}
