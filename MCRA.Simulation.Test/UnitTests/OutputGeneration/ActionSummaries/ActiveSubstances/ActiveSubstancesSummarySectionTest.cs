using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstancesSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test ActiveSubstancesSummarySection view
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesSummarySection_Test() {
            var section = new ActiveSubstancesSummarySection();
            section.Records = new List<ActiveSubstanceModelRecord> {
                new ActiveSubstanceModelRecord() {
                    MembershipProbabilities = new List<ActiveSubstanceRecord>() { new ActiveSubstanceRecord()
                },
                }
            };
            AssertIsValidView(section);
        }
    }
}
