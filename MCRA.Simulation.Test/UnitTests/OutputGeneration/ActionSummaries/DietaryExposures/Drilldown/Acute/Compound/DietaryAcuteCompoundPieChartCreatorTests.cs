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

            var mockData = new List<IndividualSubstanceDrillDownRecord>(){
                new(){SubstanceName = "Cmp1", Exposure = 100},
                new(){SubstanceName = "Cmp2", Exposure = 12},
                new(){SubstanceName = "Cmp3", Exposure = 32},
                new(){SubstanceName = "Cmp4", Exposure = 3},
                new(){SubstanceName = "Cmp5", Exposure = 22},
                new(){SubstanceName = "Cmp6", Exposure = 8},
                new(){SubstanceName = "Cmp7", Exposure = 5},
                new(){SubstanceName = "Cmp8", Exposure = 8},
            };
            var chart = new DietaryAcuteCompoundPieChartCreator(mockData, 11);
            TestRender(chart, $"TestCreate", ChartFileType.Png);
        }
    }
}