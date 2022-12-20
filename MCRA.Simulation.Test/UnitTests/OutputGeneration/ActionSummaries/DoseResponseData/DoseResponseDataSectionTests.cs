using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseData {

    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseData
    /// </summary>
    [TestClass]
    public class DoseResponseDataSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseDataSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseDataSection_Test1() {
            var section = new DoseResponseDataSection();
            section.Records = new List<DoseResponseExperimentSection>();
            AssertIsValidView(section);
        }
    }
}
