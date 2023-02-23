using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.ConcentrationDistributions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the ConcentrationDistributions action
    /// </summary>
    [TestClass]
    public class ConcentrationDistributionsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ConcentrationDistributions action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ConcentrationDistributionsActionCalculator_TestLoad() {
            var seed = 1;
            var substances = MockSubstancesGenerator.Create(3);
            var foods = MockFoodsGenerator.Create(3);

            var compiledData = new CompiledData() {
                AllConcentrationDistributions = MockConcentrationDistributionsGenerator
                    .Create(foods, substances, seed)
                    .ToList(),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new ConcentrationDistributionsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.ConcentrationDistributions);
        }
    }
}