using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresScreeningCalculation {

    /// <summary>
    /// ChronicScreeningCalculator tests.
    /// </summary>
    [TestClass]
    public class ChronicScreeningCalculatorTests {
        /// <summary>
        /// ChronicScreeningCalculator, summarizes results
        /// </summary>
        [TestMethod]
        public void ChronicScreeningCalculator_Tests1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(8);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);

            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, mu, sigma, useFraction, lor, sampleSize);
            var compoundResidueCollections = concentrationModels.Select(c => c.Value.Residues).ToList();

            var section = new ChronicScreeningCalculator(95, 95, 0, false);

            var screeningResult = section.Calculate(foodConversionResults, individualDays, foodConsumptions, compoundResidueCollections, memberships, null);
            Assert.IsTrue(screeningResult.EffectiveCumulativeSelectionPercentage > 95);
            Assert.IsTrue(screeningResult.SelectedNumberOfSccRecords > 0);
        }
    }
}
