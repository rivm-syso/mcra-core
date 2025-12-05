using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DoseResponseModels;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the DoseResponseModels action
    /// </summary>
    [TestClass]
    public class DoseResponseModelsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Test load data method of dose response models action calculator.
        /// </summary>
        [TestMethod]
        public void DoseResponseModelsActionCalculator_TestLoadData() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var responses = FakeResponsesGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllDoseResponseModels = FakeDoseResponseModelGenerator
                    .Create(substances, responses, random).ToDictionary(c => c.IdDoseResponseModel),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var calculator = new DoseResponseModelsActionCalculator(project);

            var data = new ActionData();
            var subsetManager = new SubsetManager(dataManager, project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.HasCount(2, data.DoseResponseModels);
        }

        /// <summary>
        /// Test compute method of dose response models action calculator.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        public void DoseResponseModelsActionCalculator_TestCompute(int numSubstances) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var testId = $"DoseResponseModels_{numSubstances}";

            var responses = FakeResponsesGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(numSubstances);
            var effects = FakeEffectsGenerator.Create(2);
            var experiments = FakeDoseResponseExperimentsGenerator.Create(substances, responses);
            var effectRepresentations = FakeEffectRepresentationsGenerator.Create(effects, responses);

            var project = new ProjectDto();
            var data = new ActionData() {
                Responses = responses.ToDictionary(c => c.Code),
                AllCompounds = substances,
                AllEffects = effects,
                ReferenceSubstance = substances.First(),
                FocalEffectRepresentations = effectRepresentations.Where(c => c.Effect == effects.First()).ToList(),
                SelectedResponseExperiments = experiments.Take(1).ToList()
            };

            var calculator = new DoseResponseModelsActionCalculator(project);

            // Run nominal
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, testId);

            // Run uncertainty
            var randomSources = calculator.GetRandomSources().ToArray();
            var factorialSet = new UncertaintyFactorialSet(randomSources);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}
