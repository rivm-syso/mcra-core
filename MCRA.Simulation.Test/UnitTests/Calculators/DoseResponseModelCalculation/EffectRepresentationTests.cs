﻿using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DoseResponseModelCalculation {
    /// <summary>
    /// DoseResponseModelCalculation calculator
    /// </summary>
    [TestClass]
    public class EffectRepresentationTests {
        /// <summary>
        /// Load effect representations: TestEffectRepresentations.xlsx
        /// </summary>
        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void LoadEffectRepresentations() {
            var outputPath = TestUtilities.CreateTestOutputPath("EffectRepresentationsTests");
            var sourceFileName = Path.Combine("Resources", "TestEffectRepresentations.xlsx");
            var dataFolder = Path.Combine(outputPath, "TestEffectRepresentations");
            TestUtilities.CopyRawDataTablesToFolder(sourceFileName, dataFolder);
            var targetFileName = Path.Combine(outputPath, "TestEffectRepresentations.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);

            var effectRepresentations = dataManager.GetAllEffectRepresentations();
            Assert.IsTrue(effectRepresentations.Any());
        }
    }
}
