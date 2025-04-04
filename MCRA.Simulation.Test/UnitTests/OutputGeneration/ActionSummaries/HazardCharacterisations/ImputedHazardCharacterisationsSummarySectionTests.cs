﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    ///  OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class ImputedHazardCharacterisationsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test ImputedTargetDosesSummarySection view
        /// </summary>
        [TestMethod]
        public void ImputedHazardCharacterisationsSummarySection_Test1() {
            var section = new ImputedHazardCharacterisationsSummarySection() {
                Records = [],
            };
            AssertIsValidView(section);
        }
    }
}