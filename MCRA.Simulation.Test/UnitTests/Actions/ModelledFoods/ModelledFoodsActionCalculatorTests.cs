using MCRA.Utils.Statistics;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ModelledFoods;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ModelledFoods action
    /// </summary>
    [TestClass]
    public class ModelledFoodsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ModelledFoods action: run, summarize action result and updata simulation data method.
        /// Compute from sample-based-concentrations.
        /// project.ConversionSettings.DeriveModelledFoodsFromSampleBasedConcentrations = true;
        /// </summary>
        [TestMethod]
        public void ModelledFoodsActionCalculator_TestComputeFromConcentrations() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(5);

            var data = new ActionData {
                AllFoods = foods,
                ModelledSubstances = substances,
                ActiveSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(foods.Take(5).ToList(), substances, random)
            };

            var project = new ProjectDto();
            project.GetModuleConfiguration<ModelledFoodsModuleConfig>().DeriveModelledFoodsFromSampleBasedConcentrations = true;
            var calculator = new ModelledFoodsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeFromConcentrations");

            var concentrationFoods = data.ActiveSubstanceSampleCollections.Keys.ToList();
            CollectionAssert.AreEquivalent(concentrationFoods, data.ModelledFoods.ToList());
        }

        /// <summary>
        /// Runs the ModelledFoods action: run, summarize action result and updata simulation data method
        /// Compute from maximum residue limits.
        /// project.ConversionSettings.UseWorstCaseValues = true;
        /// </summary>
        [TestMethod]
        public void ModelledFoodsActionCalculator_TestComputeFromMrls() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(5);
            var data = new ActionData() {
                AllFoods = foods,
                MaximumConcentrationLimits = MockMaximumConcentrationLimitsGenerator.Create(foods.Take(5).ToList(), substances, random)
            };

            var project = new ProjectDto();
            project.GetModuleConfiguration<ModelledFoodsModuleConfig>().DeriveModelledFoodsFromSampleBasedConcentrations = false;
            project.GetModuleConfiguration<ModelledFoodsModuleConfig>().UseWorstCaseValues = true;
            var calculator = new ModelledFoodsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeFromMrls");

            var mrlFoods = data.MaximumConcentrationLimits.Select(r => r.Key.Food).Distinct().ToList();
            CollectionAssert.AreEquivalent(mrlFoods, data.ModelledFoods.ToList());
        }

        /// <summary>
        /// Runs the ModelledFoods action: run, summarize action result and updata simulation data method
        /// Compute from maximum residue limits.
        /// project.ConversionSettings.DeriveModelledFoodsFromSingleValueConcentrations = true;
        /// </summary>
        [TestMethod]
        public void ModelledFoodsActionCalculator_TestComputeFromSingleValueConcentrations() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(5);
            var activeSubstanceSingleValueConcentrations = MockSingleValueConcentrationModelsGenerator
                .Create(foods.Take(5).ToList(), substances, random);

            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstanceSingleValueConcentrations = activeSubstanceSingleValueConcentrations
            };

            var project = new ProjectDto();
            project.GetModuleConfiguration<ModelledFoodsModuleConfig>().DeriveModelledFoodsFromSampleBasedConcentrations = false;
            project.GetModuleConfiguration<ModelledFoodsModuleConfig>().DeriveModelledFoodsFromSingleValueConcentrations = true;
            var calculator = new ModelledFoodsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeFromSingleValueConcentrations");

            var concentrationFoods = data.ActiveSubstanceSingleValueConcentrations.Select(r => r.Key.Food).Distinct().ToList();
            CollectionAssert.AreEquivalent(concentrationFoods, data.ModelledFoods.ToList());
        }
    }
}