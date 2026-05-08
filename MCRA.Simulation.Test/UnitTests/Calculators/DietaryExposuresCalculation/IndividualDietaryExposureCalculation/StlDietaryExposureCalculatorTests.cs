using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation {

    [TestClass]
    public class StlDietaryExposureCalculatorTests {

        [TestMethod]
        public void StlDietaryExposureCalculator_TestMultipleSubstances() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptions = FakeConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day));
            var consumptionsCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            foreach (var item in consumptions) {
                consumptionsCache[item.Key] = [.. item];
            }
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(foods, substances);
            var stlNumberOfDays = 3;

            var residueGenerator = new MeanConcentrationResidueGenerator(concentrationModels);
            residueGenerator.Initialize();
            var calculator = new StlDietaryExposureCalculator(
                substances,
                consumptionsCache,
                null,
                null,
                residueGenerator,
                stlNumberOfDays
            );

            var dietaryIndividualDayIntakes = calculator.CalculateDietaryIntakes(
                simulatedIndividualDays,
                new ProgressState(),
                seed
            );
            var totalExposure = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(rpfs, memberships, false));
            Assert.IsFalse(double.IsNaN(totalExposure));

            Assert.AreEqual(dietaryIndividualDayIntakes.Count, stlNumberOfDays * individuals.Count);
        }
    }
}
