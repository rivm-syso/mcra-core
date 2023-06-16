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
            var record = new SingleValueRisksThresholdExposureRatioRecord { 
                Risks = draws,
                AdjustedRisks = drawsAdjusted,
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5
            };
            var section = new SingleValueRisksThresholdExposureRatioSection() {
                Records = new List<SingleValueRisksThresholdExposureRatioRecord>() { record}
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
            var record = new SingleValueExposureThresholdRatioIndexRecord { 
                Risks = draws ,
                AdjustedRisks = drawsAdjusted,
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5
            };
            var section = new SingleValueRisksExposureThresholdRatioSection() {
                Records = new List<SingleValueExposureThresholdRatioIndexRecord>() { record }
            };
            var chart = new SingleValueRisksHIUncertaintyChartCreator(section);
            RenderChart(chart, $"TestCreateBoxPlotHiUncertainty");
        }
    }
}
