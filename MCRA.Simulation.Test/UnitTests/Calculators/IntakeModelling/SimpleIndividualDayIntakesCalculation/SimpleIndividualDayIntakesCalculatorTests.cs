using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling.IndividualAmountsCalculation {

    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class SimpleIndividualDayIntakesCalculatorTests {

        /// <summary>
        /// Test compute individual amounts.
        /// </summary>
        [TestMethod]
        public void SimpleIndividualDayIntakesCalculator_TestAll() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(
                individualDays,
                foods,
                substances,
                0.5,
                true,
                random
            );
            var calculator = new SimpleIndividualDayIntakesCalculator(substances, rpfs, memberships, true, null);
            var amounts = calculator.ComputeIndividualAmounts(exposures);
            Assert.HasCount(individuals.Count, amounts);
        }

        /// <summary>
        /// Test compute individual amounts for selected food categories.
        /// </summary>
        [TestMethod]
        public void SimpleIndividualDayIntakesCalculator_TestPerFoodAsMeasured() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(
                individualDays,
                foods,
                substances,
                0.5,
                true,
                random
            );
            var categories = new List<List<Food>>() {
                foods.Take(1).ToList(),
                foods.Skip(1).ToList()
            };
            foreach (var category in categories) {
                var calculator = new SimpleIndividualDayIntakesCalculator(substances, rpfs, memberships, true, category);
                var amounts = calculator.ComputeIndividualAmounts(exposures);
                Assert.HasCount(individuals.Count, amounts);
            }
        }
    }
}