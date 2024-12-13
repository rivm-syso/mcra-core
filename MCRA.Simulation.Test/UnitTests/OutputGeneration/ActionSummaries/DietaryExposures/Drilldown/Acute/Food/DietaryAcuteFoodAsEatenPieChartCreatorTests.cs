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

            var mockData = new List<DietaryIntakeSummaryPerFoodRecord>(){
                new(){FoodName = "Food1", Concentration = 1, IntakePerMassUnit = 10},
                new(){FoodName = "Food2", Concentration = 1, IntakePerMassUnit = 12},
                new(){FoodName = "Food3", Concentration = 1, IntakePerMassUnit = 32},
                new(){FoodName = "Food4", Concentration = 1, IntakePerMassUnit = 3},
                new(){FoodName = "Food5", Concentration = 1, IntakePerMassUnit = 22},
                new(){FoodName = "Food6", Concentration = 1, IntakePerMassUnit = 8},
                new(){FoodName = "Food7", Concentration = 1, IntakePerMassUnit = 5},
                new(){FoodName = "Food8", Concentration = 1, IntakePerMassUnit = 8},
            };
            var record = new DietaryAcuteDrillDownRecord() {
                IntakeSummaryPerFoodAsEatenRecords = mockData,
            };

            var chart = new DietaryAcuteFoodAsEatenPieChartCreator(record);
            RenderChart(chart, $"TestCreate");
        }
    }
}