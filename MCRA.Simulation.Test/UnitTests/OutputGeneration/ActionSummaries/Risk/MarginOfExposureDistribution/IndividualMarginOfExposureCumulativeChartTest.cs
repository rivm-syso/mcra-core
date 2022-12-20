using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeMarginOfExposure
    /// </summary>
    [TestClass]
    public class IndividualMarginOfExposureCumulativeChartTest : ChartCreatorTestBase {

        private int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart without uncertainty
        /// </summary>
        [TestMethod]
        public void IndividualMarginOfExposureCumulativeChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new MarginOfExposureDistributionSection() {
                IMOEDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
            };

            var chart = new MarginOfExposureCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void IndividualMarginOfExposureCumulativeChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new MarginOfExposureDistributionSection() {
                IMOEDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true)
            };

            var chart = new MarginOfExposureCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
