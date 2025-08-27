using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ConcentrationDistributions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

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
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(3);

            var compiledData = new CompiledData() {
                AllConcentrationDistributions = FakeConcentrationDistributionsGenerator
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