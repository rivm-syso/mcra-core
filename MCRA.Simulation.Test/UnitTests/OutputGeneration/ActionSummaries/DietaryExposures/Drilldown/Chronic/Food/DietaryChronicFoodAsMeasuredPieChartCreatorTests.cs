using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Chronic, Food
    /// </summary>
    [TestClass()]
    public class DietaryChronicFoodAsMeasuredPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryChronicFoodAsMeasuredPieChartCreatorTest() {

            var mockData = new List<DietaryIntakeSummaryPerFoodRecord>(){
                new(){FoodName = "Food1", IntakePerMassUnit= 30},
                new(){FoodName = "Food2", IntakePerMassUnit = 12},
                new(){FoodName = "Food3", IntakePerMassUnit = 32},
                new(){FoodName = "Food4", IntakePerMassUnit = 3},
                new(){FoodName = "Food5", IntakePerMassUnit = 22},
                new(){FoodName = "Food6", IntakePerMassUnit = 8},
                new(){FoodName = "Food7", IntakePerMassUnit = 5},
                new(){FoodName = "Food8", IntakePerMassUnit = 8},
            };
            var result = new List<DietaryDayDrillDownRecord>() { new() {
                IntakeSummaryPerFoodAsMeasuredRecords = mockData,
                }
            };

            var record = new DietaryChronicDrillDownRecord() {
                DayDrillDownRecords = result,
            };

            var chart = new DietaryChronicFoodAsMeasuredPieChartCreator(record);
            RenderChart(chart, $"TestCreate");
        }
    }
}