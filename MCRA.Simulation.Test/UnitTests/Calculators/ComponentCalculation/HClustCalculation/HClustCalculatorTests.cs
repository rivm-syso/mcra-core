using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.HClustCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {

    /// <summary>
    /// HclustsCalculation calculator
    /// </summary>
    [TestClass]
    public class HClustCalculatorTests : CalculatorTestsBase {

        /// <summary>
        /// Tests Hclusts components clustering method.
        /// </summary>
        [TestMethod]
        public void HClustCalculation_TestCompute() {
            var outputPath = GetCalculatorTestOutputFolder("TestCompute");
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
            var individualMatrix = new IndividualMatrix() {
                VMatrix = exposureMatrix.Exposures,
                SimulatedIndividuals = exposureMatrix.SimulatedIndividuals,
            };
            var calculator = new HClustCalculator(3, false, outputPath);
            var clusterResult = calculator.Compute(individualMatrix, new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1));
            var result = clusterResult.Clusters.Select(c => c.SimulatedIndividuals.Count).ToList();
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(24, result[1]);
            Assert.AreEqual(3, result[2]);
        }

        /// <summary>
        /// Tests Hclusts components clustering method.
        /// </summary>
        [TestMethod]
        public void HClustCalculation_Test1Compute() {
            var outputPath = GetCalculatorTestOutputFolder("TestCompute1");
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
            var individualMatrix = new IndividualMatrix() {
                VMatrix = exposureMatrix.Exposures,
                SimulatedIndividuals = exposureMatrix.SimulatedIndividuals,
            };
            var calculator = new HClustCalculator(2, true, outputPath);
            var clusterResult = calculator.Compute(individualMatrix, new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1));
            var result = clusterResult.Clusters.Select(c => c.SimulatedIndividuals.Count).ToList();
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(19, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(5, result[3]);


        }
    }
}
