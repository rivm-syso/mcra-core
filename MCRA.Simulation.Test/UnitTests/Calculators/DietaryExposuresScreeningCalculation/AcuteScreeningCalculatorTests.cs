using System.Linq;
using MCRA.General;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresScreeningCalculation {

    /// <summary>
    /// AcuteScreeningCalculator tests.
    /// </summary>
    [TestClass]
    public class AcuteScreeningCalculatorTests {
        /// <summary>
        /// AcuteScreeningCalculator, summarizes results and charts
        /// </summary>
        [TestMethod]
        public void AcuteScreeningCalculator_Tests1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(8);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);

            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods.Skip(4).ToList(), substances, ConcentrationModelType.Empirical, mu, sigma, useFraction, lor, sampleSize);
            var compoundResidueCollections = concentrationModels.Select(c => c.Value.Residues).ToList();

            var section = new AcuteScreeningCalculator(95, 95, 0, false);

            var screeningResult = section.Calculate(foodConversionResults, individualDays, foodConsumptions, compoundResidueCollections, memberships, null);
            Assert.IsTrue(screeningResult.EffectiveCumulativeSelectionPercentage > 95);
            Assert.IsTrue(screeningResult.SelectedNumberOfSccRecords > 0);

            var section1 = new ScreeningSummarySection();
            section1.Summarize(screeningResult, 95, 95, 0);

            var chart = new ScreeningPieChartCreator(section1);
            chart.CreateToPng(TestUtilities.ConcatWithOutputPath("Screening_test1.png"));
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("Screening_test1.svg"));

            var chart1 = new GroupedScreeningPieChartCreator(section1);
            chart1.CreateToPng(TestUtilities.ConcatWithOutputPath("GroupedScreening_test1.png"));
            chart1.CreateToSvg(TestUtilities.ConcatWithOutputPath("GroupedScreening_test1.svg"));
        }
    }
}
