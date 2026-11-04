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
        public void FoodSubstanceConversionFactorsActionCalculator_TestLoadSummarize() {
            var foods = FakeFoodsGenerator.Create(4);
            var substances = FakeSubstancesGenerator.Create(3);
            var activeSubstances = substances.Take(2);      // First two
            var measuredSubstances = substances.Skip(1);    // Last two (second is both measured and active)
            var conversionFactors = createFakeConversionFactors(foods, activeSubstances, measuredSubstances, skipFaction: 0.2, seed: 1);
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

        private static List<DeterministicSubstanceConversionFactor> createFakeConversionFactors(
            List<Food> foods,
            IEnumerable<Compound> activeSubstances,
            IEnumerable<Compound> measuredSubstances,
            double skipFaction = 0.2,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var conversionFactors = new List<DeterministicSubstanceConversionFactor>();
            foreach (var food in foods) {
                foreach (var measuredSubstance in measuredSubstances) {
                    foreach (var activeSubstance in activeSubstances) {
                        if (random.NextDouble() < skipFaction) {
                            // Also skip a fraction of the combinations
                            continue;
                        }
                        var conversionFactor = new DeterministicSubstanceConversionFactor() {
                            ActiveSubstance = activeSubstance,
                            MeasuredSubstance = measuredSubstance,
                            ConversionFactor = random.NextDouble(0.5, 2),
                            Food = food
                        };
                        conversionFactors.Add(conversionFactor);
                    }
                }
            }

            return conversionFactors;
        }
    }
}