using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class UpperDistributionFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredPieChart_Test1() {

            var mockData = new List<DistributionFoodRecord>(){
                new(){FoodName = "AppleAppleApplple40", Contribution = 10},
                new(){FoodName = "Apple2", Contribution = 12},
                new(){FoodName = "Apple3", Contribution = 32},
                new(){FoodName = "Apple4", Contribution = 3},
                new(){FoodName = "Apple5", Contribution = 22},
                new(){FoodName = "Apple6", Contribution = 8},
                new(){FoodName = "Apple7", Contribution = 5},
                new(){FoodName = "Apple8", Contribution = 8},
            };
            var section = new UpperDistributionFoodAsMeasuredSection() {
                Records = mockData,
            };

            var chart = new UpperDistributionFoodAsMeasuredPieChartCreator(section, section.Records, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
