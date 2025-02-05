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

            var mockData = new List<IndividualSubstanceDrillDownRecord>(){
                new(){SubstanceName = "Cmp1", EquivalentExposure = 10},
                new(){SubstanceName = "Cmp2", EquivalentExposure = 12},
                new(){SubstanceName = "Cmp3", EquivalentExposure = 32},
                new(){SubstanceName = "Cmp4", EquivalentExposure = 3},
                new(){SubstanceName = "Cmp5", EquivalentExposure = 22},
                new(){SubstanceName = "Cmp6", EquivalentExposure = 8},
                new(){SubstanceName = "Cmp7", EquivalentExposure = 5},
                new(){SubstanceName = "Cmp8", EquivalentExposure = 8},
            };
            var chart = new DietaryChronicCompoundPieChartCreator(mockData, 0);
            RenderChart(chart, $"TestCreate");
        }
    }
}