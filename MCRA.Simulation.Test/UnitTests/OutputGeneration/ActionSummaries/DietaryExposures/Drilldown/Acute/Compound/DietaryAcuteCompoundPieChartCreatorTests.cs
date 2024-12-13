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
                new(){CompoundName = "Compound1", DietaryIntakeAmountPerBodyWeight = 100},
                new(){CompoundName = "Compound2", DietaryIntakeAmountPerBodyWeight = 12},
                new(){CompoundName = "Compound3", DietaryIntakeAmountPerBodyWeight = 32},
                new(){CompoundName = "Compound4", DietaryIntakeAmountPerBodyWeight = 3},
                new(){CompoundName = "Compound5", DietaryIntakeAmountPerBodyWeight = 22},
                new(){CompoundName = "Compound6", DietaryIntakeAmountPerBodyWeight = 8},
                new(){CompoundName = "Compound7", DietaryIntakeAmountPerBodyWeight = 5},
                new(){CompoundName = "Compound8", DietaryIntakeAmountPerBodyWeight = 8},
            };
            var dietaryAcuteDrillDownRecord = new DietaryAcuteDrillDownRecord() {
                IntakeSummaryPerCompoundRecords = mockData,
            };

            var chart = new DietaryAcuteCompoundPieChartCreator(dietaryAcuteDrillDownRecord);
            TestRender(chart, $"TestCreate", ChartFileType.Png);
        }
    }
}