using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.InterSpeciesConversions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the InterSpeciesConversion action
    /// </summary>
    [TestClass]
    public class InterSpeciesConversionActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the InterSpeciesConversion action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void InterSpeciesConversionActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(3);
            var referenceCompound = substances.First();

            var compiledData = new CompiledData() {
                AllInterSpeciesFactors = FakeInterSpeciesFactorsGenerator.Create(substances, effects.First(), "Rat", random),
                AllSubstances = substances.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.InterSpeciesConversionsSettings.UseInterSpeciesConversionFactors = true;
            project.SubstancesSettings.CodeReferenceSubstance = substances.First().Code;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                ActiveSubstances = substances,
                AllEffects = effects,
                ReferenceSubstance = referenceCompound,
            };
            var calculator = new InterSpeciesConversionsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.InterSpeciesFactors);
            Assert.IsNotNull(data.InterSpeciesFactorModels);

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = [UncertaintySource.InterSpecies]
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.InterSpecies] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }
    }
}