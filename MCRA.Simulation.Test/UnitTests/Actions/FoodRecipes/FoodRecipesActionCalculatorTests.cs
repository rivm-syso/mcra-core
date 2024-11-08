using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.FoodRecipes;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the FoodRecipes action
    /// </summary>
    [TestClass]
    public class FoodRecipesActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the FoodRecipes action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FoodRecipesActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(6);
            var processingTypes =FakeProcessingTypesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllFoodTranslations = FakeFoodTranslationsGenerator.Create(foods, random),
            };

            var project = new ProjectDto();
            var data = new ActionData() {
                AllFoods = foods,
                ProcessingTypes = processingTypes,
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new FoodRecipesActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.FoodRecipes);
        }
    }
}