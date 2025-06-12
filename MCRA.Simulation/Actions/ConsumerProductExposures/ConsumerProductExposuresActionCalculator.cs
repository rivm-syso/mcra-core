using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {

    [ActionType(ActionType.ConsumerProductExposures)]
    public class ConsumerProductExposuresActionCalculator(ProjectDto project) : ActionCalculatorBase<ConsumerProductExposuresActionResult>(project) {
        private ConsumerProductExposuresModuleConfig ModuleConfig => (ConsumerProductExposuresModuleConfig)_moduleSettings;

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumerProductExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override ConsumerProductExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of dust exposure determinants
            var determinantsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.CPE_ConsumerProductExposureDeterminants)
            );

            return compute(
                data,
                localProgress,
                determinantsRandomGenerator
            );
        }

        protected override ConsumerProductExposuresActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of dust exposure determinants
            var determinantsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.CPE_ConsumerProductExposureDeterminants));

            return compute(
                data,
                localProgress,
                determinantsRandomGenerator,
                factorialSet,
                uncertaintySourceGenerators
            );
        }

        protected override void updateSimulationDataUncertain(ActionData data, ConsumerProductExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        private ConsumerProductExposuresActionResult compute(
            ActionData data,
            ProgressState localProgress,
            IRandom determinantsRandomGenerator,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var result = new ConsumerProductExposuresActionResult();

            // Create individual days
            localProgress.Update("Generating individual days", 30);
            var individualsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(
                    ModuleConfig.RandomSeed,
                    (int)RandomSource.CPE_DrawIndividuals
                )
            );

            var populationGeneratorFactory = new PopulationGeneratorFactory(
                ExposureType.Chronic,
                ModuleConfig.IsSurveySampling,
                ModuleConfig.NumberOfMonteCarloIterations
            );

            var populationGenerator = populationGeneratorFactory.Create();
            var consumerProductDays = data.AllIndividualConsumerProductUseFrequencies
                .GroupBy(c => c.Individual)
                .Select(c => new IndividualDay() {
                    Individual = c.Key
                })
                .ToList();

            var simulatedIndividualDays = populationGenerator.CreateSimulatedIndividualDays(
                data.ConsumerProductIndividuals,
                consumerProductDays,
                individualsRandomGenerator
            );

            var simulatedIndividuals = simulatedIndividualDays
                .Select(s => s.SimulatedIndividual)
                .Distinct()
                .ToList();

            //TODO from interface
            var exposureRoutes = new List<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var exposureCalculator = new ConsumerProductExposureCalculator(
                data.AllIndividualConsumerProductUseFrequencies,
                data.ConsumerProductExposureFractions,
                data.ConsumerProductApplicationAmounts,
                data.AllConsumerProductConcentrations,
                data.ActiveSubstances,
                exposureRoutes
            );

            var consumerProductIndividualDayIntakes = exposureCalculator
                .Compute(
                    simulatedIndividualDays,
                    new ProgressState(localProgress.CancellationToken)
                );

            // For now, we assume consumer product exposures to be expressed in
            // - ug/day when output is expressed per person
            // - ug/kg bw/day when output is expressed per kilogram bodyweight
            var targetUnit = ModuleConfig.IsPerPerson
                ? ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerDay)
                : ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            result.ConsumerProductExposureUnit = targetUnit;
            result.ConsumerProductIndividualIntakes = consumerProductIndividualDayIntakes;
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, ConsumerProductExposuresActionResult result) {
            data.ConsumerProductExposureUnit = result.ConsumerProductExposureUnit;
            data.ConsumerProductIndividualExposures = result.ConsumerProductIndividualIntakes;
        }

        protected override void summarizeActionResult(ConsumerProductExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConsumerProductExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, ConsumerProductExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConsumerProductExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }
    }
}
