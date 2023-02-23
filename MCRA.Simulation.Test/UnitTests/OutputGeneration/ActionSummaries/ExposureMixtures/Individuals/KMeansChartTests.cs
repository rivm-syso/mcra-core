using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ExposureMixtures
    /// </summary>
    [TestClass]
    public class KMeansChartTests : ChartCreatorTestBase {

        #region Fakes

        private static IndividualMatrix fakeExposuresMatrix() {
            var random = new McraRandomGenerator(1);
            var substances = MockSubstancesGenerator.Create(10);
            var individuals = MockIndividualsGenerator.Create(50, 2, random);
            var individualComponentsMatrix = MockComponentGenerator.CreateIndividualMatrix(
                individuals.Select(r => r.Id).ToList(),
                substances,
                numberOfComponents: 4,
                numberOfZeroExposureRecords: 0,
                numberOfZeroExposureSubstances: 0,
                sigma: 1,
                seed: random.Next()
            );
            return individualComponentsMatrix;
        }

        #endregion
        /// <summary>
        /// Create charts and test Kmeans view
        /// </summary>
        [TestMethod]
        public void KMeansChart_Test1() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult1 = new ClusterRecord() {
                ClusterId = 1,
                Individuals = individualMatrix.Individuals.Take(10).ToList(),
                Indices = individualMatrix.Individuals.Take(10).Select(c => c.Id).ToList()
            };
            var clusterResult2 = new ClusterRecord() {
                ClusterId = 2,
                Individuals = individualMatrix.Individuals.Skip(10).ToList(),
                Indices = individualMatrix.Individuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = new List<ClusterRecord> { clusterResult1, clusterResult2 }
            };
            var normalizationFactorU = Enumerable.Repeat(1d, individualMatrix.VMatrix.RowDimension).ToArray();
            var section = new KMeansSection();
            section.Summarize(individualMatrix, normalizationFactorU);
            var chart = new KMeansChartCreator(section);
            RenderChart(chart, $"KMeans1");
            AssertIsValidView(section);
        }
    }
}


