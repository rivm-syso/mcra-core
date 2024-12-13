using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Chronic, Compound
    /// </summary>
    [TestClass()]
    public class DietaryChronicCompoundPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryChronicCompoundPieChartCreatorTest() {

            var mockData = new List<DietaryIntakeSummaryPerCompoundRecord>(){
                new(){CompoundName = "Compound1", DietaryIntakeAmountPerBodyWeight = 10},
                new(){CompoundName = "Compound2", DietaryIntakeAmountPerBodyWeight = 12},
                new(){CompoundName = "Compound3", DietaryIntakeAmountPerBodyWeight = 32},
                new(){CompoundName = "Compound4", DietaryIntakeAmountPerBodyWeight = 3},
                new(){CompoundName = "Compound5", DietaryIntakeAmountPerBodyWeight = 22},
                new(){CompoundName = "Compound6", DietaryIntakeAmountPerBodyWeight = 8},
                new(){CompoundName = "Compound7", DietaryIntakeAmountPerBodyWeight = 5},
                new(){CompoundName = "Compound8", DietaryIntakeAmountPerBodyWeight = 8},
            };
            var result = new List<DietaryDayDrillDownRecord>() { new() {
                DietaryIntakeSummaryPerCompoundRecords = mockData,
                }
            };

            var dietaryChronicDrillDownRecord = new DietaryChronicDrillDownRecord() {
                DayDrillDownRecords = result,
            };

            var chart = new DietaryChronicCompoundPieChartCreator(dietaryChronicDrillDownRecord);
            RenderChart(chart, $"TestCreate");
        }
    }
}