using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            var adverseOutcomePathwayNetwork = MockAdverseOutcomePathwayNetworkGenerator.SimpleFake;

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
            var adverseOutcomePathwayNetwork = MockAdverseOutcomePathwayNetworkGenerator.CyclicFake;

            var relevantEffects = adverseOutcomePathwayNetwork.GetAllEffects();
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);
            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeCyclicFake.html");
        }
    }
}
