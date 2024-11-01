using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.KMeansCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {

    /// <summary>
    /// KMeansCalculation calculator
    /// </summary>
    [TestClass]
    public class KMeansCalculatorTests : CalculatorTestsBase {

        /// <summary>
        /// Tests K-means components clustering method.
        /// </summary>
        [TestMethod]
        public void KMeansCalculation_TestCompute() {
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
            var substances = MockSubstancesGenerator.Create(numSubstances);
            var substanceTargets = substances.Select(r => (r, ExposureTarget.DefaultInternalExposureTarget)).ToList();
            var exposureMatrix = FakeExposureMatrixGenerator.CreateExposureMatrix(individualIds, substanceTargets, numComponents, zeroExposureSubstances, 0);
            var individualMatrix = new IndividualMatrix() { 
                VMatrix = exposureMatrix.Exposures,
                Individuals = exposureMatrix.Individuals,
            };
            var calculator = new KMeansCalculator(4, outputPath);
            var clustering = calculator.Compute(individualMatrix, new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1));
            var result = clustering.Clusters.Select(c => c.Individuals.Count).ToList();
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(21, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(3, result[3]);
        }
    }
}
