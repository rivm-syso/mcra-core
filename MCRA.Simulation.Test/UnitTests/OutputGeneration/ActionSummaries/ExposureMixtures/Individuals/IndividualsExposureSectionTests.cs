using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures.HClust {
    [TestClass]
    public class IndividualsExposureSectionTests : SectionTestBase {

        #region Fakes

        private static IndividualMatrix fakeExposuresMatrix() {
            var random = new McraRandomGenerator(1);
            var substances = MockSubstancesGenerator.Create(10);
            var individuals = MockIndividualsGenerator.Create(50, 2, random);
            var individualMatrix = MockComponentGenerator.CreateIndividualMatrix(
                individuals.Select(r => r.Id).ToList(),
                substances,
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
        /// Test renderign of the section view.
        /// </summary>
        [TestMethod]
        public void IndividualsExposureSection_TestValidView() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = new ClusterRecord() {
                ClusterId = 1,
                Individuals = individualMatrix.Individuals.Skip(10).ToList(),
                Indices = individualMatrix.Individuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = new List<ClusterRecord> { clusterResult }
            };
            var section = new IndividualsExposureSection();
            var uMatrix = new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1);
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();
            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                section.SummarizePerCluster(clusterId, individualMatrix, normalizationFactorU, true);
            }
            AssertIsValidView(section);
            RenderView(section, filename: "TestValidView.html");
        }

        /// <summary>
        /// Test renderign of the section view.
        /// </summary>
        [TestMethod]
        public void IndividualsExposureOverviewSection_TestValidView() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = new ClusterRecord() {
                ClusterId = 1,
                Individuals = individualMatrix.Individuals.Skip(10).ToList(),
                Indices = individualMatrix.Individuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = new List<ClusterRecord> { clusterResult }
            };
            var section = new IndividualsExposureOverviewSection();
            var uMatrix = new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1);
            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                section.Summarize(new SectionHeader(), uMatrix, individualMatrix, ClusterMethodType.Hierarchical, true, true);
            }

            Assert.AreEqual(6.799, section.SubgroupComponentSummaryRecords.First().Percentage, 1e-3); 
            Assert.AreEqual(6.700, section.SubgroupComponentSummaryRecords.First().PercentageAll, 1e-3); 
            AssertIsValidView(section);
            RenderView(section, filename: "TestValidView.html");
        }
    }
}
