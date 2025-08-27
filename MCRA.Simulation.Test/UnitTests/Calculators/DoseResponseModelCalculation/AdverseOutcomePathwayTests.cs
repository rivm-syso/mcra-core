using MCRA.Simulation.Test.Helpers;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DoseResponseModelCalculation {

    /// <summary>
    /// DoseResponseModelCalculation calculator
    /// </summary>
    [TestClass]
    public class AdverseOutcomePathwayTests {

        /// <summary>
        /// Load adverse outcome pathways: AdverseOutcomePathwayNetworkArtificial.xlsx
        /// </summary>
        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void LoadAdverseOutcomePathways() {
            var outputPath = TestUtilities.CreateTestOutputPath("AdverseOutcomePathwayTests");
            var sourceFileName = Path.Combine("Resources", "AdverseOutcomePathwayNetworkArtificial.xlsx");
            var dataFolder = Path.Combine(outputPath, "AdverseOutcomePathwayTests");
            TestUtilities.CopyRawDataTablesToFolder(sourceFileName, dataFolder);
            var targetFileName = Path.Combine(outputPath, "AdverseOutcomePathwayTests.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);

            var effectRepresentations = dataManager.GetAllEffectRepresentations();
            Assert.IsTrue(effectRepresentations.Any());

            var aopn = dataManager.GetAdverseOutcomePathwayNetworks();
            Assert.IsTrue(aopn.Any());
        }
    }
}
