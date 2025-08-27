using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstancesTableSectionTests : SectionTestBase
    {
        /// <summary>
        /// Test ActiveSubstancesTableSection view
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesTableSection_Test() {
            var section = new ActiveSubstancesTableSection();
            section.Record = new ActiveSubstanceModelRecord();
            AssertIsValidView(section);
        }
    }
}
