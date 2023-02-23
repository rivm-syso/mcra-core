using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Acute, Compound
    /// </summary>
    [TestClass()]
    public class DietaryAcuteCompoundPieChartCreatorTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod()]
        public void DietaryAcuteCompoundPieChartCreatorTest() {

            var mockData = new List<DietaryIntakeSummaryPerCompoundRecord>(){
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound1", DietaryIntakeAmountPerBodyWeight = 100},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound2", DietaryIntakeAmountPerBodyWeight = 12},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound3", DietaryIntakeAmountPerBodyWeight = 32},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound4", DietaryIntakeAmountPerBodyWeight = 3},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound5", DietaryIntakeAmountPerBodyWeight = 22},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound6", DietaryIntakeAmountPerBodyWeight = 8},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound7", DietaryIntakeAmountPerBodyWeight = 5},
                new DietaryIntakeSummaryPerCompoundRecord(){CompoundName = "Compound8", DietaryIntakeAmountPerBodyWeight = 8},
            };
            var dietaryAcuteDrillDownRecord = new DietaryAcuteDrillDownRecord() {
                IntakeSummaryPerCompoundRecords = mockData,
            };

            var chart = new DietaryAcuteCompoundPieChartCreator(dietaryAcuteDrillDownRecord);
            TestRender(chart, $"TestCreate", ChartFileType.Png);
        }
    }
}