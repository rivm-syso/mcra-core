using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation {
    /// <summary>
    /// DietaryExposuresCalculation calculator
    /// </summary>
    [TestClass]
    public class ChronicDietaryExposureCalculatorTests {

        /// <summary>
        /// DietaryExposuresCalculation, chronic, multiple substances.
        /// </summary>
        [TestMethod]
        public void ChronicDietaryExposureCalculator_TestMultipleSubstances() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptions = MockConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day));
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

            var dietaryIndividualDayIntakes = calculator.CalculateDietaryIntakes(
                simulatedIndividualDays, 
                new ProgressState(), 
                seed
            );
            var totalExposure = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(rpfs, memberships, false));
            Assert.IsTrue(!double.IsNaN(totalExposure));

            var exposurePerCompound = calculator.ComputeExposurePerCompoundRecords(dietaryIndividualDayIntakes);
            var totalExposurePerCompound = exposurePerCompound.SelectMany(c => c.Value).Sum(c => c.ExposurePerBodyWeight);
            Assert.IsTrue(!double.IsNaN(totalExposurePerCompound));
        }
    }
}
