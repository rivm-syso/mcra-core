using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.SoilExposures {

    [ActionType(ActionType.SoilExposures)]
    public class SoilExposuresActionCalculator : ActionCalculatorBase<SoilExposuresActionResult> {
        private SoilExposuresModuleConfig ModuleConfig => (SoilExposuresModuleConfig)_moduleSettings;

        public SoilExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var requireDietaryExposures = ModuleConfig.SoilExposuresIndividualGenerationMethod == SoilExposuresIndividualGenerationMethod.UseDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietaryExposures;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SoilExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override SoilExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of soil exposure determinants
            var soilExposuresDeterminantsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_SoilExposureDeterminants)
            );

            return compute(
                data,
                localProgress,
                soilExposuresDeterminantsRandomGenerator
            );
        }

        protected override void updateSimulationData(ActionData data, SoilExposuresActionResult result) {
            data.IndividualSoilExposures = result.IndividualSoilExposures;
            data.SoilExposureUnit = result.SoilExposureUnit;
        }

        protected override SoilExposuresActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of soil exposure determinants
            var soilExposuresDeterminantsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_SoilExposureDeterminants));

            return compute(
                data,
                localProgress,
                soilExposuresDeterminantsRandomGenerator,
                factorialSet,
                uncertaintySourceGenerators
            );
        }

        protected override void updateSimulationDataUncertain(ActionData data, SoilExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        private SoilExposuresActionResult compute(
            ActionData data,
            ProgressState localProgress,
            IRandom soilExposuresDeterminantsRandomGenerator,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var result = new SoilExposuresActionResult();

            ICollection<IIndividualDay> individualDays = null;
            if (ModuleConfig.SoilExposuresIndividualGenerationMethod == SoilExposuresIndividualGenerationMethod.Simulate) {
                var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawIndividuals));
                var individualsGenerator = new IndividualsGenerator();
                var daysPerIndividual = 1;
                var individuals = individualsGenerator
                    .GenerateSimulatedIndividuals(
                        data.SelectedPopulation,
                        ModuleConfig.NumberOfSimulatedIndividuals,
                        daysPerIndividual,
                        individualsRandomGenerator
                    );
                individualDays = IndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            } else {
                individualDays = data.DietaryIndividualDayIntakes
                    .GroupBy(r => r.SimulatedIndividualId, (key, g) => g.First())
                    .Cast<IIndividualDay>()
                    .ToList();
            }

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var soilConcentrationDistributions = data.SoilConcentrationDistributions;
            var soilIngestions = data.SoilIngestions
                .OrderBy(r => r.AgeLower)
                .ToList();

            // For now, we assume soil exposures to be expressed in
            // - ug/day when output is expressed per person
            // - ug/kg bw/day when output is expressed per kilogram bodyweight
            var targetUnit = ModuleConfig.IsPerPerson
                ? ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerDay)
                : ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var individualSoilExposureRecords = SoilExposureCalculator
                .ComputeSoilExposure(
                    individualDays,
                    substances,
                    soilConcentrationDistributions,
                    soilIngestions,
                    data.SoilConcentrationUnit,
                    data.SoilIngestionUnit,
                    targetUnit,
                    soilExposuresDeterminantsRandomGenerator
                );

            result.IndividualSoilExposures = individualSoilExposureRecords;
            result.SoilExposureUnit = targetUnit;

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(SoilExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SoilExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, SoilExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SoilExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }
    }
}
