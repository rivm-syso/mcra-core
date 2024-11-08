using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ExposureMixtures
    /// </summary>
    [TestClass]
    public class NetworkAnalysisChartTests : ChartCreatorTestBase {

        #region Fakes

        private static (double[,], List<string>) fakeExposuresMatrix() {
            var random = new McraRandomGenerator(1);
            var numberOfSubstances = 10;
            var substances = FakeSubstancesGenerator.Create(numberOfSubstances);
            var covarMatrix = new double[10, 10];
            for (int i = 0; i < numberOfSubstances; i++) {
                for (int j = 0; j < substances.Count; j++) {
                    var logNormal = new LogNormalDistribution(0, 1);
                    //covarMatrix[i, j] = j % numberOfSubstances == i ? logNormal.Draw(random) : 0D;
                    covarMatrix[i, j] = logNormal.Draw(random);
                }
                for (int j = substances.Count - 3; j < substances.Count; j++) {
                    covarMatrix[i, j] = 0D;
                }
            }
            return (covarMatrix, substances.Select(c => c.Code).ToList());
        }

        #endregion
        /// <summary>
        /// Create charts and test NetworkAnalysis view
        /// </summary>
        [TestMethod]
        public void NetworkAnalysisChart_Test1() {
            var (covarMatrix, substances) = fakeExposuresMatrix();
            var section = new NetworkAnalysisSection() {
                SubstanceCodes = substances,
                GlassoSelect = covarMatrix,
            };

            var chart = new NetworkAnalysisChartCreator(section);
            RenderChart(chart, $"NetworkAnalysisChart1");
            AssertIsValidView(section);
        }
    }
}


