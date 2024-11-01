using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling.IntakeModels.OIMCalculation {

    /// <summary>
    /// Tests observed individual means calculator.
    /// </summary>
    [TestClass]
    public class OIMCalculatorTests {

        /// <summary>
        /// Test calculation of OIMs for single substance.
        /// </summary>
        [TestMethod]
        public void ObservedIndividualMeansCalculator_TestSingleSubstance() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.8, true, random);
            var individualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(substances, null, null, false, null);
            var individualDayIntakes = individualDayIntakesCalculator.Compute(dietaryIndividualDayIntakes);
            var oims = OIMCalculator.CalculateObservedIndividualMeans(individualDayIntakes);
            var usualExposureTotal = oims.Sum(c => c.DietaryIntakePerMassUnit);
            Assert.AreEqual(individuals.Count, oims.Count);

            var sumTotalIntakes = dietaryIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(r => r.Average(id => id.GetSubstanceTotalExposurePerMassUnit(substances.First(), false)))
                .Sum();
            Assert.AreEqual(sumTotalIntakes, oims.Sum(r => r.DietaryIntakePerMassUnit));
        }

        /// <summary>
        /// Test calculation of OIMs for multiple substances (cumulative).
        /// </summary>
        [TestMethod]
        public void ObservedIndividualMeansCalculator_TestMultipleSubstances() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);

            var consumptions = MockConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy((r => (r.Individual, r.Day)));
            var consumptionsCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            foreach (var item in consumptions) {
                consumptionsCache[item.Key] = item.ToList();
            }

            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances);
            var residueGenerator = new MeanConcentrationResidueGenerator(concentrationModels);
            residueGenerator.Initialize();
            var calculator = new ChronicDietaryExposureCalculator(
                substances,
                null,
                consumptionsCache,
                concentrationModels,
                null,
                null,
                residueGenerator,
                false,
                false
            );
            var dietaryIndividualDayIntakes = calculator
                .CalculateDietaryIntakes(simulatedIndividualDays, new ProgressState(), seed);

            var individualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(substances, rpfs, memberships, false, null);
            var individualDayIntakes = individualDayIntakesCalculator.Compute(dietaryIndividualDayIntakes);
            var oims = OIMCalculator.CalculateObservedIndividualMeans(individualDayIntakes);

            var usualExposureTotal = oims.Sum(c => c.DietaryIntakePerMassUnit);
            Assert.IsFalse(double.IsNaN(usualExposureTotal));
        }
    }
}
