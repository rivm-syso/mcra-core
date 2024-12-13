using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsEaten
    /// </summary>
    [TestClass]
    public class UpperDistributionFoodAsEatenPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenPieChart_Test1() {

            var mockData = new List<DistributionFoodRecord>(){
                new(){FoodName = "AppleAppleAppleApppleApple40", Contribution = 10},
                new(){FoodName = "wedwqdwcczx", Contribution = 12},
                new(){FoodName = "weff       ewr", Contribution = 22},
                new(){FoodName = "Apple4", Contribution = 13},
                new(){FoodName = "Asafwf5", Contribution = 22},
                new(){FoodName = "Sanas", Contribution = 28},
                new(){FoodName = "d  wew2    ffee", Contribution = 5},
                new(){FoodName = "www", Contribution = 8},
                new(){FoodName = "AppleAppleApple", Contribution = 10},
                new(){FoodName = "wedwqdwcczx", Contribution = 12},
                new(){FoodName = "weff       ewr", Contribution = 2},
                new(){FoodName = "Apple4", Contribution = 1},
                new(){FoodName = "Asafwf5", Contribution = 2},
                new(){FoodName = "Sanas", Contribution = 2},
                new(){FoodName = "d  wew2    ffee", Contribution = 5},
                new(){FoodName = "www", Contribution = 8},
            };
            var upperDistributionFoodAsEatenSection = new UpperDistributionFoodAsEatenSection() {
                Records = mockData,
            };

            var chart = new UpperDistributionFoodAsEatenPieChartCreator(upperDistributionFoodAsEatenSection, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
