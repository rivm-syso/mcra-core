using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConcentrationModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ConcentrationModels
    /// </summary>
    [TestClass]
    public class ConcentrationModelsGraphSectionTest : SectionTestBase {

        /// <summary>
        /// Test ConcentrationModelsGraphSection view
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsGraphSection_Test1() {
            var section = new ConcentrationModelsGraphSection();
            section.ConcentrationModelRecords = [
                new ConcentrationModelRecord() {
                    SubstanceCode = "A",
                    SubstanceName = "A"
                }
            ];
            AssertIsValidView(section);
        }
    }
}