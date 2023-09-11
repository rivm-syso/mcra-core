using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis, ExposureByRouteSubstance
    /// </summary>
    [TestClass]
    public class HbmIndividualDayDistributionBySubstanceSectionTests : SectionTestBase {
        /// <summary>
        /// Test HbmIndividualDayDistributionEndpointCompoundSection view
        /// </summary>
        [TestMethod]
        public void HbmIndividualDayDistributionEndpointSubstanceSection_Test1() {
            var section = new HbmIndividualDayDistributionBySubstanceSection {
                Records = new List<HbmIndividualDayDistributionBySubstanceRecord>(),
                HbmBoxPlotRecords = new()
            };
            AssertIsValidView(section);
        }
    }
}