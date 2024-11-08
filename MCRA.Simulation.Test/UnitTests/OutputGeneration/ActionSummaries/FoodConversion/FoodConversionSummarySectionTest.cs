using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.FoodConversion {
    /// <summary>
    /// OutputGeneration, ActionSummaries, FoodConversion
    /// </summary>
    [TestClass]
    public class FoodConversionSummarySectionTest : SectionTestBase {
        /// <summary>
        /// Test TDSConversionsSection, FailedConversionsSection, UnmatchedFoodsSection,ConversionsSection view
        /// </summary>
        [TestMethod]
        public void FoodConversionSummarySection_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(8);
            var substances = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = FakeFoodConversionsGenerator.Create(foodTranslations, substances);
            var tdsFoodSampleCompositions = new List<TDSFoodSampleComposition> {
                new() {
                    Food = foods[0],
                    TDSFood = foods[1],
                    Description = "description",
                    PooledAmount = 1000,
                    Regionality = "region",
                    Seasonality = "season"
                }
            };
            var tdsConversionSection = new TDSConversionsSection();
            tdsConversionSection.Summarize(tdsFoodSampleCompositions.ToLookup(c => c.Food));
            AssertIsValidView(tdsConversionSection);
            var failedConversionSection = new UnMatchedFoodAsEatenSummarySection();
            failedConversionSection.Summarize(foodConversionResults, true);
            AssertIsValidView(failedConversionSection);
            var unmatchedFoodsSection = new UnmatchedTdsFoodsSection();
            unmatchedFoodsSection.Summarize(foodConversionResults);
            AssertIsValidView(unmatchedFoodsSection);
            var conversionSection = new ConversionsSection();
            conversionSection.Summarize(foodConversionResults, substances);

            var section = new ConversionSummarySection();
            section.Summarize(
                foodConversionResults,
                foodConversionResults.Where(r => r.FoodAsMeasured == null).ToList(),
                foodConversionResults.Select(r => r.FoodAsMeasured).ToList()
            );
            AssertIsValidView(section);
            Assert.AreEqual(20, section.NumberOfConversionPaths);
            Assert.AreEqual(4, section.NumberOfFoodsAsMeasured);
        }
    }
}
