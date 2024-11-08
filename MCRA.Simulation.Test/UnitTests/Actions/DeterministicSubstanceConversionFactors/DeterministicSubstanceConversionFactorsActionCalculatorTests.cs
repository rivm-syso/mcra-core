using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.DeterministicSubstanceConversionFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the ResidueDefinitions action
    /// </summary>
    [TestClass]
    public class DeterministicSubstanceConversionFactorsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ResidueDefinitions action: load data, summarize action result method, IsExclusive = true
        ///                                                                                only substances
        /// </summary>
        [TestMethod]
        public void DeterministicSubstanceConversionFactorsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var deterministicSubstanceConversionFactors = FakeDeterministicSubstanceConversionFactorsGenerator
                .Create(activeSubstances, measuredSubstances, random);
            var compiledData = new CompiledData() {
                AllDeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors.ToList(),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var data = new ActionData();

            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new DeterministicSubstanceConversionFactorsActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.DeterministicSubstanceConversionFactors);
        }

        /// <summary>
        /// Runs the ResidueDefinitions action: load data, summarize action result method, IsExclusive = true
        ///                                                                                 substances and foods
        /// </summary>
        [TestMethod]
        public void DeterministicSubstanceConversionFactorsActionCalculator_TestLoad2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var deterministicSubstanceConversionFactors = FakeDeterministicSubstanceConversionFactorsGenerator
                .Create(activeSubstances, measuredSubstances, random, foods: foods);
            var compiledData = new CompiledData() {
                AllDeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors.ToList(),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();

            var data = new ActionData();

            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new DeterministicSubstanceConversionFactorsActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad2");
            Assert.IsNotNull(data.DeterministicSubstanceConversionFactors);
        }
    }
}