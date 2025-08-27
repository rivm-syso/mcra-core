using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Chronic, Food
    /// </summary>
    [TestClass()]
    public class DietaryChronicFoodAsEatenPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryChronicFoodAsEatenPieChartCreatorTest() {

            var mockData = new List<IndividualFoodDrillDownRecord>(){
                new(){FoodName = "Food1", Exposure= 100},
                new(){FoodName = "Food2", Exposure = 12},
                new(){FoodName = "Food3", Exposure = 32},
                new(){FoodName = "Food4", Exposure = 3},
                new(){FoodName = "Food5", Exposure = 22},
                new(){FoodName = "Food6", Exposure = 8},
                new(){FoodName = "Food7", Exposure = 5},
                new(){FoodName = "Food8", Exposure = 8},
            };
            var chart = new DietaryChronicFoodAsEatenPieChartCreator(mockData, 0);
            RenderChart(chart, $"TestCreate");
        }
    }
}