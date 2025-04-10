﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsEaten
    /// </summary>
    [TestClass]
    public class TotalDistributionFoodAsEatenPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>F
        [TestMethod]
        public void TotalDistributionFoodAsEatenPieChart_Test1() {

            var mockData = new List<DistributionFoodRecord>(){
                new(){FoodName = "AppleAppleAppleAppleAppleAppleAppleApple40", Contribution = 100},
                new(){FoodName = "Apple2", Contribution = 12},
                new(){FoodName = "Apple3", Contribution = 32},
                new(){FoodName = "Apple4", Contribution = 3},
                new(){FoodName = "Apple5", Contribution = 22},
                new(){FoodName = "Apple6", Contribution = 8},
                new(){FoodName = "Apple7", Contribution = 5},
                new(){FoodName = "Apple8", Contribution = 8},
            };
            var totalDistributionFoodAsEatenSection = new TotalDistributionFoodAsEatenSection() {
                Records = mockData,
            };

            var chart = new TotalDistributionFoodAsEatenPieChartCreator(totalDistributionFoodAsEatenSection, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
