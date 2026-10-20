using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.BackgroundFoodConcentrations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    [TestClass]
    public class BackgroundFoodConcentrationsActionCalculatorTests : ActionCalculatorTestsBase {

        [TestMethod]
        public void BackgroundFoodConcentrationsActionCalculator_TestLoad() {
            var seed = 1;
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(3);
            var backgroundConcentrations = FakeBackgroundFoodConcentrationsGenerator
                .Create(foods.Take(2), substances.Take(2), seed: seed);
            var compiledData = new CompiledData() {
                AllBackgroundFoodConcentrations = backgroundConcentrations
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new BackgroundFoodConcentrationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.BackgroundFoodConcentrations);
        }
    }
}