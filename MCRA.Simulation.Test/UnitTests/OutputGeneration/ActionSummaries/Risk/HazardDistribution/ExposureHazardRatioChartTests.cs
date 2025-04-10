﻿using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class HazardExposureRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void HazardExposureRatioDistributionChart_Test1() {
            var number = 5000;
            var logData = NormalDistribution.NormalSamples(number, .65, 1.75).ToList();
            var bins = simulateBins(logData);
            var section = new RiskRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 10,
                RiskMetricType = RiskMetricType.ExposureHazardRatio
            };
            var chart = new RiskRatioChartCreator(section);
            RenderChart(chart, "TestCreate");
        }

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, data.Count).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
    }
}
