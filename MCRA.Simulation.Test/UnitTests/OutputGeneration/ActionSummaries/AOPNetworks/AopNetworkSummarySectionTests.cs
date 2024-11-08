using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.AOPNetworks {

    /// <summary>
    /// OutputGeneration, ActionSummaries, AOPNetworks.
    /// </summary>
    [TestClass]
    public class AopNetworkSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test AopNetworksSummarySection view.
        /// </summary>
        [TestMethod]
        public void AOPNetworksSummarySection_TestSummarizeSimpleFake() {
            var section = new AopNetworkSummarySection();
            var adverseOutcomePathwayNetwork = FakeAdverseOutcomePathwayNetworkGenerator.SimpleFake;

            var relevantEffects = adverseOutcomePathwayNetwork.GetAllEffects();
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);
            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeSimpleFake.html");
        }

        /// <summary>
        /// Summarize, test AopNetworksSummarySection view.
        /// </summary>
        [TestMethod]
        public void AOPNetworksSummarySection_TestSummarizeCyclicFake() {
            var section = new AopNetworkSummarySection();
            var adverseOutcomePathwayNetwork = FakeAdverseOutcomePathwayNetworkGenerator.CyclicFake;

            var relevantEffects = adverseOutcomePathwayNetwork.GetAllEffects();
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);
            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeCyclicFake.html");
        }
    }
}
