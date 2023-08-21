using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.SingleValueRisks {

    /// <summary>
    /// OutputGeneration, ActionSummaries, SingleValueRisks
    /// </summary>
    [TestClass]
    public class SingleValueRisksUncertaintyChartCreatorTest : ChartCreatorTestBase {

        /// <summary>
        /// Test SingleValueRisk view
        /// </summary>
        [TestMethod]
        public void SingleValueRisksUncertaintyMOEChartCreatorTest_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draws = new List<double>();
            var drawsAdjusted = new List<double>();
            for (int i = 0; i < 200; i++) {
                var draw = Math.Exp(NormalDistribution.Draw(random, .5, .1));
                draws.Add(draw);
                drawsAdjusted.Add(draw * 2.3);
            }
            var record = new SingleValueRisksHazardExposureRatioRecord { 
                Risks = draws,
                AdjustedRisks = drawsAdjusted,
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5
            };
            var section = new SingleValueRisksHazardExposureRatioSection() {
                Records = new List<SingleValueRisksHazardExposureRatioRecord>() { record}
            };
            var chart = new SingleValueRisksMOEUncertaintyChartCreator(section);
            RenderChart(chart, $"TestCreateBoxPlotMOEUncertainty");
        }

        /// <summary>
        /// Test SingleValueRisk view
        /// </summary>
        [TestMethod]
        public void SingleValueRisksUncertaintyHIChartCreatorTest_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draws = new List<double>();
            var drawsAdjusted = new List<double>();
            for (int i = 0; i < 200; i++) {
                var draw = 1 / Math.Exp(NormalDistribution.Draw(random, .5, .1));
                draws.Add(draw);
                drawsAdjusted.Add(draw / 2.3);
            }
            var record = new SingleValueRisksExposureHazardRatioRecord { 
                Risks = draws ,
                AdjustedRisks = drawsAdjusted,
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5
            };
            var section = new SingleValueRisksExposureHazardRatioSection() {
                Records = new List<SingleValueRisksExposureHazardRatioRecord>() { record }
            };
            var chart = new SingleValueRisksExposureHazardRatioUncertaintyChartCreator(section);
            RenderChart(chart, $"TestCreateBoxPlotHiUncertainty");
        }
    }
}
