﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            section.Records = [];
            AssertIsValidView(section);
        }
    }
}