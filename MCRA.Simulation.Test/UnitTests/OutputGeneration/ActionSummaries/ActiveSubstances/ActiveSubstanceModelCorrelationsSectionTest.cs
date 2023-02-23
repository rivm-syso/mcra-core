using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstanceModelCorrelationsSectionTests : SectionTestBase {
        /// <summary>
        /// Test ActiveSubstanceModelCorrelationsSection view
        /// </summary>
        [TestMethod]
        public void ActiveSubstanceModelCorrelationsSection_Test() {
            var section = new ActiveSubstanceModelCorrelationsSection {
                ModelNames = new List<string>() { "A", "B" },
                PearsonCorrelations = new List<List<double>> { new List<double>() { 1, 2 } },
                SpearmanCorrelations = new List<List<double>> { new List<double>() { 1, 2 } }
            };
            //RenderView(section, filename: "AssessmentGroupMembershipModelCorrelationsSection_Test.html");
            AssertIsValidView(section);
        }
    }
}
