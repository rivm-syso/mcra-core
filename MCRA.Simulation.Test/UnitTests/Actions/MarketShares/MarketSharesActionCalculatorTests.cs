using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.MarketShares;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the MarketShares action
    /// </summary>
    [TestClass]
    public class MarketSharesActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the MarketShares action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void MarketSharesActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllMarketShares = foods.Select(c => new MarketShare() { Food = c, Percentage = random.NextDouble() * 100, BrandLoyalty = random.NextDouble() }).ToList(),
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllFoods = foods,
            };
            var calculator = new MarketSharesActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.MarketShares);
        }
    }
}