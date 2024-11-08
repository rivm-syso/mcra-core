using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                Records = []
            };
            section.Records.Add(new ProcessingTypeSummaryRecord() { });
            AssertIsValidView(section);
        }
    }
}