using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.FoodConversions;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the FoodConversion action
    /// </summary>
    [TestClass]
    public class FoodConversionActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the FoodConversion action: run and summarize action result method
        /// project.ConversionSettings.UseWorstCaseValues = true;
        /// </summary>
        [TestMethod]
        public void FoodConversionActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(20, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances, ConcentrationModelType.Empirical);
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var settings = new ModelledFoodsInfosCalculatorSettings(new () {
                DeriveModelledFoodsFromSampleBasedConcentrations = true,
                DeriveModelledFoodsFromSingleValueConcentrations = false,
                UseWorstCaseValues = false,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
            });
            var modelledFoodscalculator = new ModelledFoodsInfosCalculator(settings);
            var substanceSampleStatistics = modelledFoodscalculator.Compute(modelledFoods, substances, activeSubstanceSampleCollections, null, null);
            var modelledFoodInfos = substanceSampleStatistics.ToLookup(r => r.Food);
            var maximumResidueLimits = new Dictionary<(Food, Compound), ConcentrationLimit>();
            var foodExtrapolations = new Dictionary<Food, ICollection<Food>>();

            var data = new ActionData() {
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                AllFoodsByCode = modelledFoods.ToDictionary(c => c.Code),
                ModelledFoodInfos = modelledFoodInfos,
                FoodsAsEaten = modelledFoods
            };
            var project = new ProjectDto();
            project.FoodConversionsSettings.UseWorstCaseValues = true;

            var calculator = new FoodConversionsActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"FoodConversions");
            Assert.IsNotNull(data.FoodsAsEaten);
            Assert.IsNotNull(data.FoodConversionResults);
        }
    }
}