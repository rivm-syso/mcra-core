using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.FoodSubstanceConversionFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the FoodSubstanceConversionFactors action
    /// </summary>
    [TestClass]
    public class FoodSubstanceConversionFactorsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the FoodSubstanceConversionFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FoodSubstanceConversionFactorsActionCalculator_ReplaceFactTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(2);
            var conversionFactor = new DeterministicSubstanceConversionFactor() { 
                ActiveSubstance = substances[0],
                MeasuredSubstance = substances[1],
                ConversionFactor =3,
                Food = foods[0]
            };
            var conversionFactors = new List<DeterministicSubstanceConversionFactor>() { conversionFactor };
            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllFoodSubstanceConversionFactors = conversionFactors,
            });

            var project = new ProjectDto();
           
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances,
                AllFoods = foods
            };

            var calculator = new FoodSubstanceConversionFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
        }
    }
}