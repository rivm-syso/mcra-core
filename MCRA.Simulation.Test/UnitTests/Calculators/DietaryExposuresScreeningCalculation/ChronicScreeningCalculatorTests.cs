using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

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
            var foods = FakeFoodsGenerator.Create(8);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = FakeFoodConversionsGenerator.Create(foodTranslations, substances);

            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, mu, sigma, useFraction, lor, sampleSize);
            var compoundResidueCollections = concentrationModels.Select(c => c.Value.Residues).ToList();

            var section = new ChronicScreeningCalculator(95, 95, 0, false);

            var screeningResult = section.Calculate(foodConversionResults, individualDays, foodConsumptions, compoundResidueCollections, memberships, null);
            Assert.IsGreaterThan(95, screeningResult.EffectiveCumulativeSelectionPercentage);
            Assert.IsGreaterThan(0, screeningResult.SelectedNumberOfSccRecords);
        }
    }
}
