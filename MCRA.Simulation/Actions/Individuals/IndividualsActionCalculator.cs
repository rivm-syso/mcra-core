using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.Individuals {

    [ActionType(ActionType.Individuals)]
    public class IndividualsActionCalculator(ProjectDto project) : ActionCalculatorBase<IndividualsActionResult>(project) {
        private IndividualsModuleConfig ModuleConfig => (IndividualsModuleConfig)_moduleSettings;

        protected override void verify() {
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleSimulatedIndividuals) {
                result.Add(UncertaintySource.SimulatedIndividuals);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new IndividualsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override IndividualsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new IndividualsActionResult();
            var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.SIM_DrawIndividuals));
            var individualsGenerator = new IndividualsGenerator();
            var daysPerIndividual = 1;
            var individuals = individualsGenerator
                .GenerateSimulatedIndividuals(
                    data.SelectedPopulation,
                    ModuleConfig.NumberOfSimulatedIndividuals,
                    daysPerIndividual,
                    individualsRandomGenerator
                );
            result.Individuals = IndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            localProgress.Update(100);
            return result;
        }

        protected override IndividualsActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new IndividualsActionResult();
            if (factorialSet.Contains(UncertaintySource.SimulatedIndividuals)) {
                localProgress.Update("Resampling individuals");
                var simulatedIndividual = data.Individuals
                    .Select(c => c.SimulatedIndividual)
                    .Resample(uncertaintySourceGenerators[UncertaintySource.SimulatedIndividuals])
                    .Select((ind, ix) => new SimulatedIndividual(ind.Individual, ix))
                    .ToList();
                result.Individuals = IndividualDaysGenerator.CreateSimulatedIndividualDays(simulatedIndividual);
            } else {
                result.Individuals = data.Individuals;
            }
            return result;
        }

        protected override void updateSimulationData(ActionData data, IndividualsActionResult result) {
            data.Individuals = result.Individuals;
        }

        protected override void summarizeActionResult(IndividualsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new IndividualsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
