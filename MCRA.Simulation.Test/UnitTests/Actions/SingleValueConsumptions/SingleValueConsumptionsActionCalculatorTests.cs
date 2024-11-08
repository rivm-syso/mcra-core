using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SingleValueConsumptions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the SingleValueConsumptions action
    /// </summary>
    [TestClass]
    public class SingleValueConsumptionsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the single value consumptions action as data.
        /// </summary>
        [TestMethod]
        public void SingleValueConsumptionsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var populations = FakePopulationsGenerator.Create(1);
            var allPopulationConsumptionSingleValues = FakePopulationConsumptionsSingleValuesGenerator
                .Create(populations.First(), foods, random, ConsumptionValueType.MeanConsumption);
            var compiledData = new CompiledData() {
                AllPopulationConsumptionSingleValues = allPopulationConsumptionSingleValues,
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SingleValueConsumptionsActionCalculator(project);

            var data = new ActionData();
            calculator.LoadData(data, subsetManager, new CompositeProgressState());

            var header = new SummaryToc();
            calculator.SummarizeActionResult(null, data, header, 0, new CompositeProgressState());
            WriteReport(header, "TestLoad.html");
        }

        /// <summary>
        /// Runs the single value consumptions action as data.
        /// </summary>
        [TestMethod]
        public void SingleValueConsumptionsActionCalculator_TestLoadProcessed() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var populations = FakePopulationsGenerator.Create(1);
            var allPopulationConsumptionSingleValues = FakePopulationConsumptionsSingleValuesGenerator
                .Create(populations.First(), foods, random, ConsumptionValueType.MeanConsumption);
            var compiledData = new CompiledData() {
                AllPopulationConsumptionSingleValues = allPopulationConsumptionSingleValues,
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SingleValueConsumptionsActionCalculator(project);

            var data = new ActionData();
            calculator.LoadData(data, subsetManager, new CompositeProgressState());

            var header = new SummaryToc();
            calculator.SummarizeActionResult(null, data, header, 0, new CompositeProgressState());
            WriteReport(header, "TestLoadProcessed.html");
        }

        /// <summary>
        /// Runs the single value consumptions action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueConsumptionsActionCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var project = new ProjectDto();
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var individualDays = FakeIndividualDaysGenerator.Create(100, 2, false, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays);
            var data = new ActionData() {
                AllFoodsByCode = foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                ModelledFoodConsumerDays = individualDays,
                ConsumptionsByModelledFood = consumptionsByModelledFood
            };
            var calculator = new SingleValueConsumptionsActionCalculator(project);
            project.CalculationActionTypes.Add(ActionType.SingleValueConsumptions);
            TestRunUpdateSummarizeNominal(project, calculator, data, "ConsumptionsByModelledFood_1");
            Assert.AreEqual(3, data.SingleValueConsumptionModels.Count);
        }

        /// <summary>
        /// Runs the single value consumptions action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueConsumptionsActionCalculator_TestComputeProcessed() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var project = new ProjectDto();
            project.CalculationActionTypes.Add(ActionType.SingleValueConsumptions);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var individualDays = FakeIndividualDaysGenerator.Create(100, 2, false, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays);
            var data = new ActionData() {
                AllFoodsByCode = foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                ModelledFoodConsumerDays = individualDays,
                ConsumptionsByModelledFood = consumptionsByModelledFood
            };
            var calculator = new SingleValueConsumptionsActionCalculator(project);
            project.SingleValueConsumptionsSettings.IsProcessing = true;
            TestRunUpdateSummarizeNominal(project, calculator, data, "ConsumptionsByModelledFood_1");
            Assert.AreEqual(12, data.SingleValueConsumptionModels.Count);
        }
    }
}
