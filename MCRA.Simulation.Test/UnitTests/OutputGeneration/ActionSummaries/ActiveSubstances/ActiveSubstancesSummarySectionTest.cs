using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            section.Records = new List<ActiveSubstanceModelRecord>();
            section.Records.Add(new ActiveSubstanceModelRecord() {
                MembershipProbabilities = new List<ActiveSubstanceRecord>() { new ActiveSubstanceRecord()
                },
            });
            AssertIsValidView(section);
        }
    }
}
