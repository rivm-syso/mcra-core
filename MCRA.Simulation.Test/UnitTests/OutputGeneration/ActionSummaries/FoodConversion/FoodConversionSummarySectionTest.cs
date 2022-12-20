using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
            var foods = MockFoodsGenerator.Create(8);
            var substances = MockSubstancesGenerator.Create(3);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);
            var tdsFoodSampleCompositions = new List<TDSFoodSampleComposition>();
            tdsFoodSampleCompositions.Add(new TDSFoodSampleComposition() {
                Food = foods[0],
                TDSFood = foods[1],
                Description = "description",
                PooledAmount = 1000,
                Regionality = "region",
                Seasonality = "season"
            });
            var tdsConversionSection = new TDSConversionsSection();
            tdsConversionSection.Summarize(tdsFoodSampleCompositions.ToLookup(c => c.Food, c => c));
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
