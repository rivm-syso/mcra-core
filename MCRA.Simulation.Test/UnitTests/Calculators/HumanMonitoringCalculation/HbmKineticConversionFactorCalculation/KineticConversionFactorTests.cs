using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using System;
using MCRA.Simulation.Test.Helpers;
using MCRA.Utils.Test;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactorCalculation {

    /// <summary>
    /// KineticConversionFactor calculator
    /// </summary>
    [TestClass]
    public class KineticConversionFactorTests {

        /// <summary>
        /// Load adverse outcome pathways: KineticConversionFactor.xlsx
        /// </summary>
        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void LoadAKineticConversionFactorTests() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var outputPath = TestUtilities.CreateTestOutputPath("KineticConversionFactorTests");
            var sourceFileName = Path.Combine("Resources", Path.Combine("KineticConversionFactors", "KineticConversionFactor.xlsx"));
            var dataFolder = Path.Combine(outputPath, "KineticConversionFactorTests");
            TestResourceUtilities.CopyRawDataTablesToFolder(sourceFileName, dataFolder);
            var targetFileName = Path.Combine(outputPath, "KineticConversionFactorTests.zip");
            var dataManager = TestResourceUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);

            var kineticConversionFactors = dataManager.GetAllKineticConversionFactors();
            Assert.IsTrue(kineticConversionFactors.Any());
            foreach (var record in kineticConversionFactors) {
                var model = KineticConversionFactorCalculatorFactory.Create(record, false);
                var draw = model.Draw(random, 19, GenderType.Male);
            }
        }
    }
}
