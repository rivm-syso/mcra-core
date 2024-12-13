using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Acute, Food
    /// </summary>
    [TestClass()]
    public class DietaryAcuteFoodAsMeasuredPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryAcuteFoodAsMeasuredPieChartCreatorTest() {

            var mockData = new List<DietaryIntakeSummaryPerFoodRecord>(){
                new(){FoodName = "Food1", Concentration = 1, IntakePerMassUnit = 10},
                new(){FoodName = "Food2", Concentration = 1, IntakePerMassUnit = 12},
                new(){FoodName = "Food3", Concentration = 1, IntakePerMassUnit = 32},
                new(){FoodName = "Food4", Concentration = 1, IntakePerMassUnit = 3},
                new(){FoodName = "Food5", Concentration = 1, IntakePerMassUnit = 22},
                new(){FoodName = "Food6", Concentration = 1, IntakePerMassUnit = 18},
                new(){FoodName = "Food7", Concentration = 1, IntakePerMassUnit = 15},
                new(){FoodName = "Food8", Concentration = 1, IntakePerMassUnit = 8},
            };
            var record = new DietaryAcuteDrillDownRecord() {
                IntakeSummaryPerFoodAsMeasuredRecords = mockData,
            };

            var chart = new DietaryAcuteFoodAsMeasuredPieChartCreator(record);
            RenderChart(chart, $"TestCreate");
        }
    }
}