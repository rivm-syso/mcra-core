using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis, ExposureByRouteSubstance
    /// </summary>
    [TestClass]
    public class HbmIndividualDistributionBySubstanceRecordTests : SectionTestBase {
        /// <summary>
        /// Test HbmIndividualDistributionEndpointSubstanceSection view
        /// </summary>
        [TestMethod]
        public void HbmIndividualDistributionEndpointSubstanceSection_Test1() {
            var section = new HbmIndividualDistributionBySubstanceSection {
                IndividualRecords = [],
                HbmBoxPlotRecords = []
            };
            AssertIsValidView(section);
        }
    }
}