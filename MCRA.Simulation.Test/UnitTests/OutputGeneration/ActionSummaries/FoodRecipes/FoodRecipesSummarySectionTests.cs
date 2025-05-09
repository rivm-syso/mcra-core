﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.FoodRecipes {
    /// <summary>
    /// OutputGeneration, ActionSummaries, FoodRecipes
    /// </summary>
    [TestClass]
    public class FoodRecipesSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test FoodRecipesSummarySection view
        /// </summary>
        [TestMethod]
        public void FoodRecipesSummarySection_Test1() {
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas", "Fruit", "Cocktail", "IceCream", "Pizza", "Sauce", "Tomato");
            var recipes = new List<FoodTranslation> {
                new(foods[3], foods[0], 10),
                new(foods[3], foods[1], 10),
                new(foods[3], foods[2], 140),
                new(foods[4], foods[1], 230),
                new(foods[4], foods[2], 10),
                new(foods[5], foods[0], 30),
                new(foods[6], foods[7], 30),
                new(foods[7], foods[8], 45)
            };
            var processingTypes = new List<ProcessingType>();
            var section = new FoodRecipesSummarySection();
            section.Summarize(recipes, foods, processingTypes);
            Assert.AreEqual(7, section.Records.Count);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test FoodRecipesSummarySection view
        /// </summary>
        [TestMethod]
        public void FoodRecipesSummarySection_Test2() {
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas", "Fruit", "Cocktail", "IceCream", "Pizza", "Sauce", "Tomato");
            var recipes = new List<FoodTranslation> {
                new(foods[0], foods[1], 10),
                new(foods[1], foods[2], 10),
                new(foods[2], foods[3], 140),
                new(foods[3], foods[4], 230),
                new(foods[4], foods[5], 10),
                new(foods[5], foods[6], 30),
                new(foods[6], foods[7], 30),
                new(foods[7], foods[8], 45)
            };
            var processingTypes = new List<ProcessingType>();
            var section = new FoodRecipesSummarySection();
            section.Summarize(recipes, foods, processingTypes);
            Assert.AreEqual(1, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}