using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

            var mockData = new List<DietaryIntakeSummaryPerFoodRecord>(){
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food1", IntakePerMassUnit= 100},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food2", IntakePerMassUnit = 12},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food3", IntakePerMassUnit = 32},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food4", IntakePerMassUnit = 3},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food5", IntakePerMassUnit = 22},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food6", IntakePerMassUnit = 8},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food7", IntakePerMassUnit = 5},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food8", IntakePerMassUnit = 8},
            };
            var result = new List<DietaryDayDrillDownRecord>() { new DietaryDayDrillDownRecord() {
                IntakeSummaryPerFoodAsEatenRecords = mockData,
                }
            };

            var record = new DietaryChronicDrillDownRecord() {
                DayDrillDownRecords = result,
            };

            var chart = new DietaryChronicFoodAsEatenPieChartCreator(record);
            RenderChart(chart, $"TestCreate");
        }
    }
}