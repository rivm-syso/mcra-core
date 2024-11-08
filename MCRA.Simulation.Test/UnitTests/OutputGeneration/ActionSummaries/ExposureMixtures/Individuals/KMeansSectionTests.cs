using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.ComponentCalculation.KMeansCalculation;
using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures.KMeans {
    [TestClass]
    public class KMeansSectionTests : SectionTestBase {

        #region Fakes

        private static IndividualMatrix fakeExposuresMatrix() {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(10);
            var substanceTargets = substances
                .Select(r => (r, ExposureTarget.DefaultInternalExposureTarget))
                .ToList();
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var individualMatrix = FakeExposureMatrixGenerator.CreateIndividualMatrix(
                individuals.Select(r => r.Id).ToList(),
                substanceTargets,
                numberOfComponents: 4,
                numberOfZeroExposureRecords: 0,
                numberOfZeroExposureSubstances: 0,
                sigma: 1,
                seed: random.Next()
            );
            return individualMatrix;
        }

        #endregion

        /// <summary>
        /// Test summarize section based on fake exposures matrix.
        /// </summary>
        [TestMethod]
        public void KMeansSection_TestSummarize() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = new ClusterRecord() {
                ClusterId = 2,
                Individuals = individualMatrix.Individuals.Skip(10).ToList(),
                Indices = individualMatrix.Individuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = [clusterResult]
            };
            var normalizationFactorU = Enumerable.Repeat(1d, individualMatrix.VMatrix.RowDimension).ToArray();
            var section = new KMeansSection();
            section.Summarize(individualMatrix, normalizationFactorU);
        }

        /// <summary>
        /// Test renderign of the section view.
        /// </summary>
        [TestMethod]
        public void KMeansSection_TestValidView() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var calculator = new KMeansCalculator(3);
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = calculator.Compute(individualMatrix, new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1));
            var section = new KMeansSection() {
                Clusters = clusterResult.Clusters.Select(c => c.Individuals.Count).ToList(),
                IndividualCodes = individualMatrix.Individuals.Select(c => c.Code).ToList(),
                ComponentCodes = Enumerable.Range(1, individualMatrix.NumberOfComponents).Select(c => c.ToString()).ToList(),
                VMatrix = individualMatrix.VMatrix,
                ClusterResult = clusterResult
            };
            AssertIsValidView(section);
            RenderView(section, filename: "TestValidView.html");
        }
    }
}
