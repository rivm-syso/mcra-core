using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringData {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringData, Samples
    /// </summary>
    [TestClass]
    public class HbmSamplesSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test HumanMonitoringSamplesSummarySection view
        /// </summary>
        [TestMethod]
        public void HbmSamplesSummarySection_TestValidView() {
            var section = new HbmSamplesSummarySection();
            section.Records = new List<HbmSamplesSummaryRecord>();
            AssertIsValidView(section);
        }
    }
}