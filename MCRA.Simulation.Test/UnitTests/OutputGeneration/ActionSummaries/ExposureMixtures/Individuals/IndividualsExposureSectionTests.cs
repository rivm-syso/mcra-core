using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures{
    [TestClass]
    public class IndividualsExposureSectionTests : ChartCreatorTestBase {

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
        /// Test rendering of the section view.
        /// </summary>
        [TestMethod]
        public void IndividualsExposureSection_TestValidView() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = new ClusterRecord() {
                ClusterId = 1,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Skip(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = [clusterResult]
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
        /// Test rendering of the section view.
        /// </summary>
        [TestMethod]
        public void IndividualsExposureSection_BoxPlot() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult1 = new ClusterRecord() {
                ClusterId = 1,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Skip(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Skip(10).Select(c => c.Id).ToList()
            };
            var clusterResult2 = new ClusterRecord() {
                ClusterId = 2,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Take(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Take(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = [clusterResult1, clusterResult2]
            };
            var section = new IndividualsExposureSection();
            var uMatrix = new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1);
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();
            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                section.SummarizeBoxPlotPerCluster(clusterId, individualMatrix, normalizationFactorU);
            }
            AssertIsValidView(section);
            var chart = new ComponentClusterBoxPlotChartCreator(section);
            RenderChart(chart, $"TestBoxPlot");

            RenderView(section, filename: "TestValidView.html");
        }

        /// <summary>
        /// Test rendering of the section view.
        /// </summary>
        [TestMethod]
        public void IndividualsExposureOverviewSection_TestValidView() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult = new ClusterRecord() {
                ClusterId = 1,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Skip(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = [clusterResult]
            };
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            var section = new IndividualsExposureOverviewSection();
            var uMatrix = new GeneralMatrix(1, individualMatrix.VMatrix.RowDimension, 1);
            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                section.Summarize(
                    new SectionHeader(),
                    uMatrix,
                    individualMatrix,
                    ClusterMethodType.Hierarchical,
                    true,
                    true
                );
            }

            Assert.IsGreaterThan(0, section.SubgroupComponentSummaryRecords.First().Percentage);
            Assert.IsGreaterThan(0, section.SubgroupComponentSummaryRecords.First().PercentageAll);
            AssertIsValidView(section);
            RenderView(section, filename: "TestValidView.html");
        }
    }
}
