using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.IntraSpeciesFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the IntraSpeciesFactors action
    /// </summary>
    [TestClass]
    public class IntraSpeciesFactorsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the IntraSpeciesFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = MockEffectsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(3);
            var referenceCompound = substances.First();

            var compiledData = new CompiledData() {
                AllIntraSpeciesFactors = MockIntraSpeciesFactorsGenerator.Create(substances, effects.First(), random),
                AllSubstances = substances.ToDictionary(c => c.Code),
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                ActiveSubstances = substances,
                AllEffects = effects,
                ReferenceSubstance = referenceCompound,
            };
            var calculator = new IntraSpeciesFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.IntraSpeciesFactors);
            Assert.IsNotNull(data.IntraSpeciesFactorModels);

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = new List<UncertaintySource>() { UncertaintySource.IntraSpecies }
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.IntraSpecies] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }
    }
}