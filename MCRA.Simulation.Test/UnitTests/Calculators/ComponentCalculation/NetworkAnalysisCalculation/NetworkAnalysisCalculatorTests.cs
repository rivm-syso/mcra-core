using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {
    /// <summary>
    /// NetworkAnalysisCalculation calculator
    /// </summary>
    [TestClass]
    public class NetworkAnalysisCalculatorTests : CalculatorTestsBase {

        /// <summary>
        /// Tests NetworkAnalysis components clustering method.
        /// </summary>
        [TestMethod]
        public void NetworkAnalysisCalculation_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numIndividuals = 30;
            var zeroExposureIndividuals = random.Next(0, numIndividuals / 2);
            var numSubstances = 10;
            var zeroExposureSubstances = numSubstances > 10 ? random.Next(0, numSubstances / 5) : 0;
            var numComponents = random.Next(1, numSubstances - zeroExposureSubstances);
            var individuals = FakeIndividualsGenerator.Create(numIndividuals, 1, random);
            var individualIds = individuals.Select(r => r.Id).ToList();
            var substances = FakeSubstancesGenerator.Create(numSubstances);
            var substanceTargets = substances.Select(r => (r, ExposureTarget.DefaultInternalExposureTarget)).ToList();
            var exposureMatrix = FakeExposureMatrixGenerator.CreateExposureMatrix(individualIds, substanceTargets, numComponents, zeroExposureSubstances, 0);
            var calculator = new NetworkAnalysisCalculator(false);
            var glassoSelect = calculator.Compute(exposureMatrix.Exposures);
            Assert.AreEqual(0.664, glassoSelect[0,0], 1e-2);
        }
    }
}
