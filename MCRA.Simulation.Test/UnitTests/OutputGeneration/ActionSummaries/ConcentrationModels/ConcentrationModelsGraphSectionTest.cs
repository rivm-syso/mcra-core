using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            section.ConcentrationModelRecords = new List<ConcentrationModelRecord>();
            section.ConcentrationModelRecords.Add(new ConcentrationModelRecord() {
                CompoundCode = "A",
                CompoundName = "A"
            });
            AssertIsValidView(section);
        }
    }
}