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
                new DistributionFoodRecord(){FoodName = "AppleAppleAppleApppleApple40", Contribution = 10},
                new DistributionFoodRecord(){FoodName = "wedwqdwcczx", Contribution = 12},
                new DistributionFoodRecord(){FoodName = "weff       ewr", Contribution = 22},
                new DistributionFoodRecord(){FoodName = "Apple4", Contribution = 13},
                new DistributionFoodRecord(){FoodName = "Asafwf5", Contribution = 22},
                new DistributionFoodRecord(){FoodName = "Sanas", Contribution = 28},
                new DistributionFoodRecord(){FoodName = "d  wew2    ffee", Contribution = 5},
                new DistributionFoodRecord(){FoodName = "www", Contribution = 8},
                new DistributionFoodRecord(){FoodName = "AppleAppleApple", Contribution = 10},
                new DistributionFoodRecord(){FoodName = "wedwqdwcczx", Contribution = 12},
                new DistributionFoodRecord(){FoodName = "weff       ewr", Contribution = 2},
                new DistributionFoodRecord(){FoodName = "Apple4", Contribution = 1},
                new DistributionFoodRecord(){FoodName = "Asafwf5", Contribution = 2},
                new DistributionFoodRecord(){FoodName = "Sanas", Contribution = 2},
                new DistributionFoodRecord(){FoodName = "d  wew2    ffee", Contribution = 5},
                new DistributionFoodRecord(){FoodName = "www", Contribution = 8},
            };
            var upperDistributionFoodAsEatenSection = new UpperDistributionFoodAsEatenSection() {
                Records = mockData,
            };

            var chart = new UpperDistributionFoodAsEatenPieChartCreator(upperDistributionFoodAsEatenSection, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
