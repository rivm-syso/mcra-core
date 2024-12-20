﻿using MCRA.Simulation.OutputGeneration;
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
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food1", Concentration = 1, IntakePerMassUnit = 10},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food2", Concentration = 1, IntakePerMassUnit = 12},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food3", Concentration = 1, IntakePerMassUnit = 32},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food4", Concentration = 1, IntakePerMassUnit = 3},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food5", Concentration = 1, IntakePerMassUnit = 22},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food6", Concentration = 1, IntakePerMassUnit = 8},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food7", Concentration = 1, IntakePerMassUnit = 5},
                new DietaryIntakeSummaryPerFoodRecord(){FoodName = "Food8", Concentration = 1, IntakePerMassUnit = 8},
            };
            var record = new DietaryAcuteDrillDownRecord() {
                IntakeSummaryPerFoodAsEatenRecords = mockData,
            };

            var chart = new DietaryAcuteFoodAsEatenPieChartCreator(record);
            RenderChart(chart, $"TestCreate");
        }
    }
}