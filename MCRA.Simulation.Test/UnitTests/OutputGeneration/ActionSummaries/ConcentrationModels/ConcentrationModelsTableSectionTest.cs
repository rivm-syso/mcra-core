using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConcentrationModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ConcentrationModels
    /// </summary>
    [TestClass]
    public class ConcentrationModelsTableSectionTest : SectionTestBase {

        /// <summary>
        /// Test ConcentrationModelsTableSection view
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsTableSection_Test1() {
            var section = new ConcentrationModelsTableSection();
            section.ConcentrationModelRecords = [
                new ConcentrationModelRecord() {
                    CompoundCode = "A",
                    CompoundName = "A"
                }
            ];
            AssertIsValidView(section);
        }
    }
}