using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.FoodExtrapolations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the FoodExtrapolations action
    /// </summary>
    [TestClass]
    public class FoodExtrapolationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the FoodExtrapolations action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FoodExtrapolationsActionCalculator_Test() {
            var foods = MockFoodsGenerator.Create(10);
            var foodExtrapolations = new Dictionary<Food, ICollection<Food>> {
                [foods.ElementAt(4)] = foods.Skip(5).Take(4).ToList()
            };
            var compiledData = new CompiledData() {
                AllFoodExtrapolations = foodExtrapolations,
            };
            var project = new ProjectDto();
            var data = new ActionData();

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new FoodExtrapolationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.FoodExtrapolations);
        }
    }
}