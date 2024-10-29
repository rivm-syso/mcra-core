using MCRA.Data.Compiled.Wrappers;
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
            var requireDietaryExposures = ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.UseDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietaryExposures;
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

            // Random generator for computation of dust exposure determinants
            var dustExposuresDeterminantsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DustExposureDeterminants));

            return compute(
                data,
                localProgress,
                dustExposuresDeterminantsRandomGenerator
            );
        }

        protected override void updateSimulationData(ActionData data, DustExposuresActionResult result) {
            data.IndividualDustExposures = result.IndividualDustExposures;
        }

        protected override DustExposuresActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of dust exposure determinants
            var dustExposuresDeterminantsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DustExposureDeterminants));

            return compute(
                data,
                localProgress,
                dustExposuresDeterminantsRandomGenerator,
                factorialSet,
                uncertaintySourceGenerators
            );
        }

        protected override void updateSimulationDataUncertain(ActionData data, DustExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        private DustExposuresActionResult compute(
            ActionData data,
            ProgressState localProgress,
            IRandom dustExposuresDeterminantsRandomGenerator,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var result = new DustExposuresActionResult();

            ICollection<IIndividualDay> individuals = null;
            if (ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate) {
                var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawIndividuals));
                var individualsGenerator = new IndividualsGenerator();
                individuals = individualsGenerator
                    .GenerateIndividuals(
                        data.SelectedPopulation,
                        ModuleConfig.NumberOfSimulatedIndividuals,
                        individualsRandomGenerator
                    )
                    .Select(r => new DustIndividualDayExposure() {
                        Individual = r,
                        Day = "1",
                        SimulatedIndividualId = r.Id,
                        SimulatedIndividualDayId = r.Id,
                        IndividualSamplingWeight = r.SamplingWeight
                    })
                    .Cast<IIndividualDay>()
                    .ToList();
            } else {
                individuals = [.. data.DietaryIndividualDayIntakes];
            }

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var dustConcentrationDistributions = data.DustConcentrationDistributions;
            var dustIngestions = data.DustIngestions
                .OrderBy(r => r.AgeLower)
                .ToList();

            // TODO: reconsider to derive exposure unit based on dust concentrations and ingestions/inhalations?
            var targetUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            // TODO: exposure unit should be passed as an argument
            var dustAdherenceAmounts = data.DustAdherenceAmounts;
            var dustAvailabilityFractions = data.DustAvailabilityFractions;
            var dustBodyExposureFractions = data.DustBodyExposureFractions;

            var sampledIndividuals = individuals;
            if (ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.UseDietaryExposures) {
                // TODO: this number should come from settings. For chronic, iterate over survey, for acute
                // draw from consumption individuals (or iterate over the randomly generated individuals).
                var nInd = ModuleConfig.NumberOfSimulatedIndividuals; // matching example

                // TODO: Is this allowed or should this use a new random source?
                var dustExposuresRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawDustExposures));

                var uncertaintySourceGenerator = factorialSet?.Contains(UncertaintySource.DustExposures) ?? false
                    ? uncertaintySourceGenerators[UncertaintySource.DustExposures]
                    : dustExposuresRandomGenerator;

                sampledIndividuals = individuals
                    .DrawRandom(uncertaintySourceGenerator, ind => ind.Individual.SamplingWeight, nInd)
                    .ToList();
            }

            // TODO: convert amount to amount / bodyweight
            var individualDustExposureRecords = DustExposureCalculator
                .ComputeDustExposure(
                    sampledIndividuals,
                    substances,
                    dustConcentrationDistributions,
                    dustIngestions,
                    dustAdherenceAmounts,
                    dustAvailabilityFractions,
                    dustBodyExposureFractions,
                    ModuleConfig.SelectedExposureRoutes,
                    data.DustConcentrationUnit,
                    data.DustIngestionUnit,
                    targetUnit,
                    dustExposuresDeterminantsRandomGenerator,
                    ModuleConfig.DustTimeExposed
                );

            result.IndividualDustExposures = individualDustExposureRecords;

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
