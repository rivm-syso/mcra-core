using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {

    [ActionType(ActionType.EnvironmentalBurdenOfDisease)]
    public class EnvironmentalBurdenOfDiseaseActionCalculator : ActionCalculatorBase<EnvironmentalBurdenOfDiseaseActionResult> {
        private EnvironmentalBurdenOfDiseaseModuleConfig ModuleConfig => (EnvironmentalBurdenOfDiseaseModuleConfig)_moduleSettings;

        public EnvironmentalBurdenOfDiseaseActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
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

                foreach (var exposureEffectFunction in exposureEffectFunctions) {
                    var hbmIndividualCollection = data.HbmIndividualCollections
                        .FirstOrDefault(r => r.Target == exposureEffectFunction.ExposureTarget);

                    if (hbmIndividualCollection == null) {
                        var msg = $"Failed to compute effects for exposure effect function {exposureEffectFunction.Code}: missing estimates available for matrix {exposureEffectFunction.ExposureTarget.GetDisplayName()}.";
                        throw new Exception(msg);
                    }

                    var exposureEffectCalculator = new ExposureEffectCalculator(exposureEffectFunction);
                    var exposureEffectResults = exposureEffectCalculator.Compute(
                        hbmIndividualCollection,
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
    }
}
