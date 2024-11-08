using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ConcentrationLimits;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ConcentrationLimits action
    /// </summary>
    [TestClass]
    public class ConcentrationLimitsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the ConcentrationLimits action: : load data and summarize method
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitsActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(3);

            var compiledData = new CompiledData() {
                AllMaximumConcentrationLimits = FakeMaximumConcentrationLimitsGenerator
                    .Create(foods, substances, random)
                    .Select(c => c.Value).ToList(),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new ConcentrationLimitsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.MaximumConcentrationLimits);
        }
    }
}