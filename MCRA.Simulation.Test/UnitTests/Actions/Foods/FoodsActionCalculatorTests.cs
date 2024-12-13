using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.Foods;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Foods action
    /// </summary>
    [TestClass]
    public class FoodsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        ///  Runs the Foods action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FoodsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.CreateFoodsWithUnitWeights(20, random, .2, ["NL", "DE", "BE", "IT"]);
            var processingTypes = FakeProcessingTypesGenerator.Create(5);
            var compiledData = new CompiledData() {
                AllFoods = foods.ToDictionary(c => c.Code),
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new FoodsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.AllFoods);
            Assert.IsNotNull(data.AllFoodsByCode);
            Assert.IsNotNull(data.ProcessingTypes);
        }
    }
}