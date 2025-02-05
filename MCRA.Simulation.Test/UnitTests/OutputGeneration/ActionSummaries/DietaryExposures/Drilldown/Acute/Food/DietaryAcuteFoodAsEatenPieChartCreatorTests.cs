using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Acute, Food
    /// </summary>
    [TestClass()]
    public class DietaryAcuteFoodAsEatenPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryAcuteFoodAsEatenPieChartCreatorTest() {

            var mockData = new List<IndividualFoodDrillDownRecord>(){
                new(){FoodName = "Food1", TotalConsumption = 1, Exposure = 10},
                new(){FoodName = "Food2", TotalConsumption = 1, Exposure = 12},
                new(){FoodName = "Food3", TotalConsumption = 1, Exposure = 32},
                new(){FoodName = "Food4", TotalConsumption = 1, Exposure = 3},
                new(){FoodName = "Food5", TotalConsumption = 1, Exposure = 22},
                new(){FoodName = "Food6", TotalConsumption = 1, Exposure = 8},
                new(){FoodName = "Food7", TotalConsumption = 1, Exposure = 5},
                new(){FoodName = "Food8", TotalConsumption = 1, Exposure = 8},
            };
            var chart = new DietaryAcuteFoodAsEatenPieChartCreator(mockData, 11);
            RenderChart(chart, $"TestCreate");
        }
    }
}