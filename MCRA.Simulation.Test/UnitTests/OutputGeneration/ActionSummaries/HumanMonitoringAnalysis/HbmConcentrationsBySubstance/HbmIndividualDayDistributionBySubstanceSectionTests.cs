using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            var section = new HbmIndividualDayDistributionBySubstanceSection();
            section.Records = new List<HbmIndividualDayDistributionBySubstanceRecord>();
            section.HbmBoxPlotRecords = new List<HbmConcentrationsPercentilesRecord>();
            AssertIsValidView(section);
        }
    }
}