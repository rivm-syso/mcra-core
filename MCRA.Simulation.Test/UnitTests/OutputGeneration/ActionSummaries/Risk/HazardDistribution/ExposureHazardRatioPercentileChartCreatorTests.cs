﻿using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class ExposureHazardRatioPercentileChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void RiskPercentileChart_TestUncertainty() {
            var section = new RiskRatioPercentileSection() {
                Percentiles = mock(100),
                RiskMetricType = RiskMetricType.ExposureHazardRatio
            };
            var chart = new RiskRatioPercentileChartCreator(section);
            RenderChart(chart, "TestUncertainty");
        }

        private UncertainDataPointCollection<double> mock(int numberOfSamples) {
            var percentages = new List<double>() { 50, 90, 95, 99, 99.5, 99.9 };
            var mu = 110.5;
            var sigma = 5;
            var draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            for (int i = 0; i < 10; ++i) {
                draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
                collection.AddUncertaintyValues(draw.Percentiles(percentages));
            }
            return collection;
        }
    }
}
