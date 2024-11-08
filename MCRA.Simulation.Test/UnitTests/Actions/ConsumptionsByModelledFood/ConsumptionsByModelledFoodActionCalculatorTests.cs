using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ConsumptionsByModelledFood;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ConsumptionsByModelledFood action
    /// </summary>
    [TestClass]
    public class ConsumptionsByModelledFoodActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ConsumptionsByModelledFood action:
        /// project.SubsetSettings.ModelledFoodsConsumerDaysOnly = true;
        /// </summary>
        [TestMethod]
        public void ConsumptionsByModelledFoodActionCalculator_Test1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(2);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(modelledFoods, individualDays, random);
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var allFoods = foodsAsEaten;
            var foodSurvey = new FoodSurvey() { Code = "survey" };
            var foodTranslations = MockFoodTranslationsGenerator.Create(modelledFoods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);

            var data = new ActionData() {
            SelectedFoodConsumptions = foodConsumptions,
                ConsumerIndividuals = individuals,
                FoodConversionResults = foodConversionResults,
                FoodSurvey = foodSurvey,
                AllFoods = allFoods,
                ModelledFoods = modelledFoods,
                ConsumerIndividualDays = individualDays,
            };
            var project = new ProjectDto();
            project.ActionType = ActionType.ConsumptionsByModelledFood;
            project.ConsumptionsByModelledFoodSettings.ModelledFoodsConsumerDaysOnly = true;
            var calculator = new ConsumptionsByModelledFoodActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "ConsumptionsByModelledFood_1");
            Assert.IsNotNull(data.ModelledFoodConsumers);
            Assert.IsNotNull(data.ModelledFoodConsumerDays);
            Assert.IsNotNull(data.SelectedFoodConsumptions);
            Assert.IsNotNull(data.ConsumptionsByModelledFood);
            Assert.AreEqual(5, data.ModelledFoodConsumers.Count);
        }

        /// <summary>
        /// Runs the ConsumptionsByModelledFood action:
        /// project.SubsetSettings.ModelledFoodsConsumerDaysOnly = false;
        /// </summary>
        [TestMethod]
        public void ConsumptionsByModelledFoodActionCalculator_Test2() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(2);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(modelledFoods, individualDays, random);
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var allFoods = foodsAsEaten;
            var foodSurvey = new FoodSurvey() { Code = "survey" };
            var foodTranslations = MockFoodTranslationsGenerator.Create(modelledFoods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);

            var data = new ActionData() {
                SelectedFoodConsumptions = foodConsumptions,
                ConsumerIndividuals = individuals,
                ConsumerIndividualDays = individualDays,
                FoodConversionResults = foodConversionResults,
                FoodSurvey = foodSurvey,
                AllFoods = allFoods,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto();
            project.ConsumptionsByModelledFoodSettings.ModelledFoodsConsumerDaysOnly = false;
            var calculator = new ConsumptionsByModelledFoodActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "ConsumptionsByModelledFood_2");
            Assert.IsNotNull(data.ModelledFoodConsumers);
            Assert.IsNotNull(data.ModelledFoodConsumerDays);
            Assert.IsNotNull(data.SelectedFoodConsumptions);
            Assert.IsNotNull(data.ConsumptionsByModelledFood);
            Assert.AreEqual(5, data.ModelledFoodConsumers.Count);
        }

        /// <summary>
        /// Runs the ConsumptionsByModelledFood action
        /// project.SubsetSettings.ModelledFoodsConsumerDaysOnly = true;
        /// project.SubsetSettings.RestrictPopulationByModelledFoodSubset = true;
        /// </summary>
        [TestMethod]
        public void ConsumptionsByModelledFoodActionCalculator_Test3() {
            var project = new ProjectDto { ActionType = ActionType.ConsumptionsByModelledFood };
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(2);
            var individuals = FakeIndividualsGenerator.Create(20, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(modelledFoods, individualDays, random);
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var allFoods = foodsAsEaten;
            var foodSurvey = new FoodSurvey() { Code = "survey" };
            var foodTranslations = MockFoodTranslationsGenerator.Create(modelledFoods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);

            var data = new ActionData() {
                SelectedFoodConsumptions = foodConsumptions,
                ConsumerIndividuals = individuals,
                FoodConversionResults = foodConversionResults,
                FoodSurvey = foodSurvey,
                AllFoods = allFoods,
                ModelledFoods = modelledFoods,
                ConsumerIndividualDays = individualDays,
            };
            var config = project.ConsumptionsByModelledFoodSettings;
            config.ModelledFoodsConsumerDaysOnly = true;
            config.RestrictPopulationByModelledFoodSubset = true;
            config.FocalFoodAsMeasuredSubset = [modelledFoods[1].Code];
            var calculator = new ConsumptionsByModelledFoodActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "ConsumptionsByModelledFood_3");
            Assert.IsNotNull(data.ModelledFoodConsumers);
            Assert.IsNotNull(data.ModelledFoodConsumerDays);
            Assert.IsNotNull(data.SelectedFoodConsumptions);
            Assert.IsNotNull(data.ConsumptionsByModelledFood);
            Assert.IsTrue(data.ModelledFoodConsumers.Count > 0);
        }
    }
}