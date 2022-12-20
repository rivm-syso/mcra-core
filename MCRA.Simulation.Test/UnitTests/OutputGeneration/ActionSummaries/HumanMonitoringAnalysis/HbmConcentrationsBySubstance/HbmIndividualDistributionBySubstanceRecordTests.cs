using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            var section = new HbmIndividualDistributionBySubstanceSection();
            section.Records = new List<HbmIndividualDistributionBySubstanceRecord>();
            section.HbmBoxPlotRecords = new List<HbmConcentrationsPercentilesRecord>();
            AssertIsValidView(section);
        }
    }
}