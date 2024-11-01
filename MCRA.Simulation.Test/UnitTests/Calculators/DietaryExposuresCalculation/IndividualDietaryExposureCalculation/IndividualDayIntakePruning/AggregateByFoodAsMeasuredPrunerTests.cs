using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation {

    /// <summary>
    /// AggregateToFoodAsMeasuredPruner tests calculator
    /// </summary>
    [TestClass]
    public class AggregateByFoodAsMeasuredPrunerTests {

        /// <summary>
        /// AggregateToFoodAsMeasuredPruner
        /// </summary>
        [TestMethod]
        public void AggregateByFoodAsMeasuredPruner_Test1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foods = MockFoodsGenerator.Create(8);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);
            var individualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.2, true, random, false);

            var individualDayIntakePruner = new AggregateByFoodAsMeasuredPruner();
            foreach (var individualDayIntake in individualDayIntakes) {
                var prunedResult = individualDayIntakePruner.Prune(individualDayIntake);
                var expectedTotal = individualDayIntake.TotalExposurePerMassUnit(rpfs, memberships, false);
                var prunedTotal = individualDayIntake.TotalExposurePerMassUnit(rpfs, memberships, false);
                Assert.IsTrue(prunedResult.IntakesPerFood.Count <= foods.Count);
                Assert.AreEqual(expectedTotal, prunedTotal);
            }
        }
    }
}
