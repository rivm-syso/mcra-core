using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.DustExposures {

    [ActionType(ActionType.DustExposures)]
    public class DustExposuresActionCalculator(ProjectDto project) : ActionCalculatorBase<DustExposuresActionResult>(project) {
        private DustExposuresModuleConfig ModuleConfig => (DustExposuresModuleConfig)_moduleSettings;

        protected override void verify() {
            var requireDietaryExposures = ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.UseDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietaryExposures;
            _actionInputRequirements[ActionType.Individuals].IsRequired = !requireDietaryExposures;
            _actionInputRequirements[ActionType.Individuals].IsVisible = !requireDietaryExposures;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DustExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override DustExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of dust exposure determinants
            var dustExposuresDeterminantsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DustExposureDeterminants)
            );

            return compute(
                data,
                localProgress,
                dustExposuresDeterminantsRandomGenerator
            );
        }

        protected override void updateSimulationData(ActionData data, DustExposuresActionResult result) {
            data.IndividualDustExposures = result.IndividualDustExposures;
            data.DustExposureUnit = result.DustExposureUnit;
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

            ICollection<IIndividualDay> individualDays = null;
            if (ModuleConfig.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate) {
                individualDays = data.Individuals;
            } else {
                individualDays = data.DietaryIndividualDayIntakes
                    .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                    .Cast<IIndividualDay>()
                    .ToList();
            }
            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var dustConcentrationDistributions = data.DustConcentrationDistributions;
            var dustIngestions = data.DustIngestions
                .OrderBy(r => r.AgeLower)
                .ToList();

            // For now, we assume dust exposures to be expressed in
            // - ug/day when output is expressed per person
            // - ug/kg bw/day when output is expressed per kilogram bodyweight
            var targetUnit = ModuleConfig.IsPerPerson
                ? ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerDay)
                : ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var individualDustExposureRecords = DustExposureCalculator
                .ComputeDustExposure(
                    individualDays,
                    substances,
                    ModuleConfig.SelectedExposureRoutes,
                    dustConcentrationDistributions,
                    dustIngestions,
                    data.DustAdherenceAmounts,
                    data.DustAvailabilityFractions,
                    data.DustBodyExposureFractions,
                    data.DustConcentrationUnit,
                    data.DustIngestionUnit,
                    ModuleConfig.DustTimeExposed,
                    targetUnit,
                    dustExposuresDeterminantsRandomGenerator
                );

            result.IndividualDustExposures = individualDustExposureRecords;
            result.DustExposureUnit = targetUnit;

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
