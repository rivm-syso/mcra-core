using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.TotalDietStudyCompositions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the TotalDietStudyCompositions action
    /// </summary>
    [TestClass]
    public class TotalDietStudyCompositionsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the TotalDietStudyCompositions action: load data, summarize action result method
        /// </summary>
        [TestMethod]
        public void ResidueDefinitionsActionCalculator_Test1() {
            var foods = FakeFoodsGenerator.Create(2);
            var tDSFoodSampleCompositions = new List<TDSFoodSampleComposition> {
                new() {
                    Food = foods[0],
                    TDSFood = foods[1],
                    Description = "Description",
                    PooledAmount = 1000,
                    Regionality = "Regionality",
                    Seasonality = "Seasonality",
                }
            };
            var compiledData = new CompiledData() {
                AllTDSFoodSampleCompositions = tDSFoodSampleCompositions,
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new TotalDietStudyCompositionsActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.TdsFoodCompositions);
        }
    }
}