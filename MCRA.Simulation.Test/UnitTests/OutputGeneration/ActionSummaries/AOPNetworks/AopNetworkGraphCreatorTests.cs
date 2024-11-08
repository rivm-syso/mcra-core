using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.AOPNetworks {

    /// <summary>
    /// OutputGeneration, ActionSummaries, ConcentrationModels
    /// </summary>
    [TestClass]
    public class AopNetworkGraphCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void AopNetworkGraphCreator_TestCreateSimpleFake() {
            var section = new AopNetworkSummarySection();
            var adverseOutcomePathwayNetwork = FakeAdverseOutcomePathwayNetworkGenerator.SimpleFake;

            var relevantEffects = adverseOutcomePathwayNetwork.GetAllEffects();
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);

            var chart = new AopNetworkGraphCreator(section, 160, true);
            TestRender(chart, "TestCreate", ChartFileType.Svg);
        }
    }
}
