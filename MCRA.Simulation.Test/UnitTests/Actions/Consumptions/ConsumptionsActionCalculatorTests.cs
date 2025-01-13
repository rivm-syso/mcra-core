using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Actions.Consumptions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Consumptions action
    /// </summary>
    [TestClass]
    public class ConsumptionsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the Consumptions action: without consumptions per modelled food supplied
        /// </summary>
        [TestMethod]
        public void ConsumptionsActionCalculator_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individuals = FakeIndividualsGenerator.Create(25, 2, true, properties, random);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var foods = FakeFoodsGenerator.Create(3);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodSurveys = FakeFoodSurveysGenerator.Create(1, individuals);
            var populations = FakePopulationsGenerator.Create(1);
            var compiledData = new CompiledData() {
                AllIndividuals = individuals.ToDictionary(c => c.Code),
                AllDietaryIndividualProperties = properties.ToDictionary(r => r.Code),
                AllFoodConsumptions = foodConsumptions,
                AllFoodSurveys = foodSurveys.ToDictionary(c => c.Code),
            };

            var dataManager = new MockCompiledDataManager(compiledData);

            var config = new ConsumptionsModuleConfig {
                NameCofactor = "Gender",
                NameCovariable = "Age",
                MatchIndividualSubsetWithPopulation = IndividualSubsetType.IgnorePopulationDefinition
            };
            var project = new ProjectDto(config);
            project.PopulationsSettings.IsCompute = true;

            var data = new ActionData() {
                AllFoods = foods,
                SelectedPopulation = populations.First(),
            };
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new ConsumptionsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"Consumptions_1");
            Assert.IsNotNull(data.FoodsAsEaten);
            Assert.IsNotNull(data.FoodSurvey);
            Assert.IsNotNull(data.ConsumerIndividuals);
            Assert.IsNotNull(data.ConsumerIndividualDays);
            Assert.IsNotNull(data.Cofactor);
            Assert.IsNotNull(data.Covariable);
            Assert.IsNotNull(data.SelectedFoodConsumptions);
        }

        /// <summary>
        /// Runs the Consumptions action: with consumptions per modelled food supplied
        /// </summary>
        [TestMethod]
        public void ConsumptionsActionCalculator_Test2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individuals = FakeIndividualsGenerator.Create(25, 2, true, properties, random);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var foods = FakeFoodsGenerator.Create(3);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodSurveys = FakeFoodSurveysGenerator.Create(1, individuals);
            var populations = FakePopulationsGenerator.Create(1);

            var config = new ConsumptionsModuleConfig {
                NameCofactor = "Gender",
                NameCovariable = "Age",
                MatchIndividualSubsetWithPopulation = IndividualSubsetType.IgnorePopulationDefinition
            };
            var project = new ProjectDto(config);
            project.PopulationsSettings.IsCompute = true;

            var data = new ActionData() {
                AllFoods = foods,
                SelectedPopulation = populations.First(),
            };

            var compiledData = new CompiledData() {
                AllIndividuals = individuals.ToDictionary(c => c.Code),
                AllDietaryIndividualProperties = properties.ToDictionary(r => r.Code),
                AllFoodConsumptions = foodConsumptions,
                AllFoodSurveys = foodSurveys.ToDictionary(c => c.Code),
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new ConsumptionsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"Consumptions_2");
        }

        /// <summary>
        /// Test load data method of consumptions action. Checks whether the appropriate
        /// consumptions and individuals are loaded based on the various ways in which
        /// subsets can be made based on the consumed foods.
        /// </summary>
        [TestMethod]
        public void ConsumptionsActionCalculator_TestFocalFoodAsEatenSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individuals = FakeIndividualsGenerator.Create(7, 2, false, properties, random);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var foods = FakeFoodsGenerator.Create(4);
            var populations = FakePopulationsGenerator.Create(1);
            int[,] consumptionPatterns = {
                { 1, 0, 0, 0 }, // 1 : 1
                { 1, 0, 0, 0 }, // 1 : 2
                { 1, 1, 0, 0 }, // 2 : 1
                { 1, 1, 0, 0 }, // 2 : 2
                { 1, 1, 1, 0 }, // 3 : 1
                { 1, 1, 1, 0 }, // 3 : 2
                { 1, 1, 1, 1 }, // 4 : 1
                { 1, 1, 1, 1 }, // 4 : 2
                { 0, 0, 0, 0 }, // 5 : 1
                { 1, 1, 1, 1 }, // 5 : 2
                { 0, 0, 0, 0 }, // 6 : 1
                { 0, 0, 0, 0 }, // 6 : 2
                { 0, 0, 0, 0 }, // 7 : 1
                { 0, 0, 0, 0 }, // 7 : 2
            };

            var foodConsumptions = FakeFoodConsumptionsGenerator
                .Create(foods, individualDays, consumptionPatterns, random);
            var foodSurveys = FakeFoodSurveysGenerator.Create(1, individuals);
            var compiledData = new CompiledData() {
                AllIndividuals = individuals.ToDictionary(c => c.Code),
                AllDietaryIndividualProperties = properties.ToDictionary(r => r.Code),
                AllFoodConsumptions = foodConsumptions,
                AllFoodSurveys = foodSurveys.ToDictionary(c => c.Code)
            };

            var consumptionSubset = foods.Skip(2).Select(r => r.Code).ToArray();
            var consumerSubset = foods.Skip(2).Select(r => r.Code).ToArray();
            var scenarios = new List<(ProjectDto, (int, int, int, int))>() {
                (createTestProject(true, ExposureType.Chronic, true, false, consumptionSubset, consumerSubset), (3, 6, 4, 18)),
                (createTestProject(true, ExposureType.Chronic, false, true, consumptionSubset, consumerSubset), (3, 6, 2, 8)),
                (createTestProject(true, ExposureType.Chronic, false, false, consumptionSubset, consumerSubset), (5, 10, 4, 24)),
                (createTestProject(true, ExposureType.Chronic, false, false, null, null), (5, 10, 4, 24)),
                (createTestProject(true, ExposureType.Acute, true, false, consumptionSubset, consumerSubset), (3, 5, 4, 18)),
                (createTestProject(true, ExposureType.Acute, false, true, consumptionSubset, consumerSubset), (3, 5, 2, 8)),
                (createTestProject(true, ExposureType.Acute, false, false, consumptionSubset, consumerSubset), (5, 9, 4, 24)),
                (createTestProject(true, ExposureType.Acute, false, false, null, null), (5, 9, 4, 24)),
                (createTestProject(false, ExposureType.Chronic, true, false, consumptionSubset, consumerSubset), (7, 14, 4, 24)),
                (createTestProject(false, ExposureType.Chronic, false, true, consumptionSubset, consumerSubset), (7, 14, 2, 8)),
            };
            var count = 0;
            foreach (var (project, (consumerCount, daysCount, foodsCount, consumptionsCount)) in scenarios) {
                count++;
                var data = new ActionData {
                    AllFoods = foods,
                    AllFoodsByCode = foods.ToDictionary(r => r.Code),
                    SelectedPopulation = populations.First(),
                };

                project.ActionType = ActionType.Consumptions;
                project.PopulationsSettings.IsCompute = true;
                var dataManager = new MockCompiledDataManager(compiledData);
                var subsetManager = new SubsetManager(dataManager, project);
                var calculator = new ConsumptionsActionCalculator(project);
                TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"Consumptions_3_{count}");

                Assert.AreEqual(consumerCount, data.ConsumerIndividuals.Count);
                Assert.AreEqual(daysCount, data.ConsumerIndividualDays.Count);
                Assert.AreEqual(foodsCount, data.FoodsAsEaten.Count);
                Assert.AreEqual(consumptionsCount, data.SelectedFoodConsumptions.Count);
            }
        }

        private ProjectDto createTestProject(
            bool consumerDaysOnly = false,
            ExposureType exposureType = ExposureType.Chronic,
            bool restrictPopulationByFoodAsEatenSubset = false,
            bool restrictConsumptionsByFoodAsEatenSubset = false,
            string[] foodAsEatenSubset = null,
            string[] focalFoodAsEatenSubset = null
        ) {
            var config = new ConsumptionsModuleConfig {
                ExposureType = exposureType,
                ConsumerDaysOnly = consumerDaysOnly,
                RestrictConsumptionsByFoodAsEatenSubset = restrictConsumptionsByFoodAsEatenSubset,
                RestrictPopulationByFoodAsEatenSubset = restrictPopulationByFoodAsEatenSubset,
                FocalFoodAsEatenSubset = focalFoodAsEatenSubset?.ToList(),
                FoodAsEatenSubset = foodAsEatenSubset?.ToList(),
                MatchIndividualSubsetWithPopulation = IndividualSubsetType.IgnorePopulationDefinition
            };
            var project = new ProjectDto(config);
            return project;
        }
    }
}