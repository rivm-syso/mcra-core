using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class UpperDistributionTDSFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionTDSFoodAsMeasuredPieChart_Test1() {
            var mockData = new List<TDSReadAcrossFoodRecord>(){
                new(){FoodName = "AppleAppleApplple40", Contribution = 10},
                new(){FoodName = "Apple2", Contribution = 12},
                new(){FoodName = "Apple3", Contribution = 32},
                new(){FoodName = "Apple4", Contribution = 3},
                new(){FoodName = "Apple5", Contribution = 22},
                new(){FoodName = "Apple6", Contribution = 8},
                new(){FoodName = "Apple7", Contribution = 5},
                new(){FoodName = "Apple8", Contribution = 8},
            };
            var section = new UpperDistributionTDSFoodAsMeasuredSection() {
                Records = mockData,
            };
            var chart = new UpperDistributionTDSFoodAsMeasuredPieChartCreator(section);
            RenderChart(chart, $"TestCreate");
        }
    }
}
