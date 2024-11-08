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
            section.Records = [
                new ActiveSubstanceModelRecord() {
                    MembershipProbabilities = [ new ActiveSubstanceRecord()
                ],
                }
            ];
            AssertIsValidView(section);
        }
    }
}
