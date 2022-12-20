using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Processing {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Processing
    /// </summary>
    [TestClass]
    public class ProcessingFactorDataSectionTests : SectionTestBase {
        /// <summary>
        /// Test ProcessingFactorDataSection view
        /// </summary>
        [TestMethod]
        public void ProcessingFactorDataSection_Test1() {
            var section = new ProcessingFactorDataSection() {
                Records = new List<ProcessingFactorDataRecord>()
            };
            AssertIsValidView(section);
        }
    }
}