using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.DustExposures {

    [ActionType(ActionType.DustExposures)]
    public class DustExposuresActionCalculator : ActionCalculatorBase<DustExposuresActionResult> {
        private DustExposuresModuleConfig ModuleConfig => (DustExposuresModuleConfig)_moduleSettings;

        public DustExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var requireConsumptions = ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.UseConsumptions;
            _actionInputRequirements[ActionType.Consumptions].IsRequired = requireConsumptions;
            _actionInputRequirements[ActionType.Consumptions].IsVisible = requireConsumptions;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DustExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override DustExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
            ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress);
        }

        protected override void updateSimulationData(ActionData data, DustExposuresActionResult result) {
            data.DustExposures = result.DustExposures;
            data.DustExposureSets = result.DustExposureSets;
            data.DustExposureRoutes = result.DustExposureRoutes;
        }

        protected override DustExposuresActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress, factorialSet, uncertaintySourceGenerators);
        }

        protected override void updateSimulationDataUncertain(ActionData data, DustExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        private DustExposuresActionResult compute(
            ActionData data,
            ProgressState localProgress,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var result = new DustExposuresActionResult();
            var selectedExposureRoutes = ModuleConfig.SelectedExposureRoutes;

            var individuals = new List<Individual>();
            if (ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate) {
                var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawIndividuals));
                var individualsGenerator = new IndividualsGenerator();
                individuals = individualsGenerator
                    .GenerateIndividuals(
                        data.SelectedPopulation,
                        ModuleConfig.NumberOfSimulatedIndividuals,
                        individualsRandomGenerator
                    );
            } else {
                individuals = [.. data.ConsumerIndividuals];
            }

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var dustConcentrationDistributions = data.DustConcentrationDistributions;
            var dustIngestions = data.DustIngestions
                .OrderBy(r => r.AgeLower)
                .ToList();

            var dustAdherenceAmounts = data.DustAdherenceAmounts;
            var dustAvailabilityFractions = data.DustAvailabilityFractions;
            var dustBodyExposureFractions = data.DustBodyExposureFractions;

            var sampledIndividuals = individuals;
            if (ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.UseConsumptions) {
                // TODO: this number should come from settings. For chronic, iterate over survey, for acute
                // draw from consumption individuals (or iterate over the randomly generated individuals).
                var nInd = 2352; // matching example

                // TODO: Get variability random seed for dust exposures.
                var seed = 37; // ModuleConfig.RandomSeed?
                var uncertaintySourceGenerator = factorialSet?.Contains(UncertaintySource.DustExposures) ?? false
                    ? uncertaintySourceGenerators[UncertaintySource.DustExposures]
                    : new McraRandomGenerator(seed);

                sampledIndividuals = individuals
                    .DrawRandom(uncertaintySourceGenerator, ind => ind.SamplingWeight, nInd)
                    .ToList();
            }

            var nonDietaryExposureSets = substances
                .Select(r => {
                    var result = DustExposureCalculator
                        .ComputeDustExposure(
                            sampledIndividuals,
                            dustConcentrationDistributions,
                            dustIngestions,
                            dustAdherenceAmounts,
                            dustAvailabilityFractions,
                            dustBodyExposureFractions,
                            selectedExposureRoutes,
                            ModuleConfig,
                            r
                        );
                    return result;
                })
                .SelectMany(r => r)
                .ToList();

            var nonDietaryExposures = nonDietaryExposureSets
                .GroupBy(r => r.NonDietarySurvey)
                .ToDictionary(r => r.Key, r => r.ToList());

            result.DustExposureSets = nonDietaryExposureSets;
            result.DustExposures = nonDietaryExposures;
            result.DustExposureRoutes = selectedExposureRoutes;

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(DustExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new DustExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, DustExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new DustExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }
    }
}
