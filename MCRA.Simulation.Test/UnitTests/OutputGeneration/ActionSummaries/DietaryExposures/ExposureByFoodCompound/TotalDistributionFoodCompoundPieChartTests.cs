using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFoodCompound
    /// </summary>
    [TestClass]
    public class TotalDistributionFoodCompoundPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundPieChart_Test1() {

            var mockData = new List<DistributionFoodCompoundRecord>(){
                new(){CompoundName = "C1", FoodName = "AppleAppleAppleApe40", Contribution = 10},
                new(){CompoundName = "C2", FoodName = "Apple2", Contribution = 12},
                new(){CompoundName = "C3", FoodName = "Apple3", Contribution = 32},
                new(){CompoundName = "C4", FoodName = "Apple4", Contribution = 3},
                new(){CompoundName = "C5", FoodName = "Apple5", Contribution = 22},
                new(){CompoundName = "C6", FoodName = "Apple6", Contribution = 8},
                new(){CompoundName = "C7", FoodName = "Apple7", Contribution = 5},
                new(){CompoundName = "C8", FoodName = "Apple8", Contribution = 8},
            };
            var section = new TotalDistributionFoodCompoundSection() {
                Records = mockData,
            };

            var chart = new TotalDistributionFoodCompoundPieChartCreator(section, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
