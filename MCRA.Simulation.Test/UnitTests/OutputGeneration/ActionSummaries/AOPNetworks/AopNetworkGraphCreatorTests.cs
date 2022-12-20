using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var adverseOutcomePathwayNetwork = MockAdverseOutcomePathwayNetworkGenerator.SimpleFake;

            var relevantEffects = adverseOutcomePathwayNetwork.GetAllEffects();
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);

            var chart = new AopNetworkGraphCreator(section, 160, true);
            TestRender(chart, "TestCreate", ChartFileType.Svg);
        }
    }
}
