using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.RelativePotencyFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, RelativePotencyFactors
    /// </summary>
    [TestClass]
    public class CompoundRPFDataSectionTests : SectionTestBase {
        /// <summary>
        /// Test CompoundRPFDataSection view
        /// </summary>
        [TestMethod]
        public void CompoundRPFDataSection_Test1() {
            var section = new CompoundRPFDataSection();
            section.Records = new List<CompoundRPFDataRecord>();
            AssertIsValidView(section);
        }
    }
}