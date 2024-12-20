﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis, IndividualDaySamplingMethodSubstanceConcentration
    /// </summary>
    [TestClass]
    public class HbmIndividualDaySubstanceConcentrationsSectionTests : SectionTestBase {
        /// <summary>
        /// Test IndividualDaySamplingMethodSubstanceConcentrationSection view
        /// </summary>
        [TestMethod]
        public void IndividualDaySamplingMethodSubstanceConcentrationSection_Test1() {
            var section = new HbmIndividualDaySubstanceConcentrationsSection();
            section.Records = [];
            AssertIsValidView(section);
        }
    }
}