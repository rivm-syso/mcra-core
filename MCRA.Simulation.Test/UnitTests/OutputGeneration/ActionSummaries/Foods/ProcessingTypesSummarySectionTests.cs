using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Foods {

    /// <summary>
    /// OutputGeneration,processing types
    /// </summary>
    [TestClass]
    public class ProcessingTypesSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test FoodsSummarySection view
        /// </summary>
        [TestMethod]
        public void ProcessingTypesSummarySection_TestHasValidView() {
            var section = new ProcessingTypesSummarySection {
                Records = new List<ProcessingTypeSummaryRecord>()
            };
            section.Records.Add(new ProcessingTypeSummaryRecord() { });
            AssertIsValidView(section);
        }
    }
}