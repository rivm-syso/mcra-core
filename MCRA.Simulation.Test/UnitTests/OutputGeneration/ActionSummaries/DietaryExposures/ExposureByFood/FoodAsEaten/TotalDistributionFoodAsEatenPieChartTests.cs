using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
                new DistributionFoodRecord(){FoodName = "AppleAppleAppleAppleAppleAppleAppleApple40", Contribution = 100},
                new DistributionFoodRecord(){FoodName = "Apple2", Contribution = 12},
                new DistributionFoodRecord(){FoodName = "Apple3", Contribution = 32},
                new DistributionFoodRecord(){FoodName = "Apple4", Contribution = 3},
                new DistributionFoodRecord(){FoodName = "Apple5", Contribution = 22},
                new DistributionFoodRecord(){FoodName = "Apple6", Contribution = 8},
                new DistributionFoodRecord(){FoodName = "Apple7", Contribution = 5},
                new DistributionFoodRecord(){FoodName = "Apple8", Contribution = 8},
            };
            var totalDistributionFoodAsEatenSection = new TotalDistributionFoodAsEatenSection() {
                Records = mockData,
            };

            var chart = new TotalDistributionFoodAsEatenPieChartCreator(totalDistributionFoodAsEatenSection, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
