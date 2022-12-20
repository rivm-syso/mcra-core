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
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class HazardDistributionChartTests : ChartCreatorTestBase {
        private static int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create charts and test HazardDistributionSection view without uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var bins = simulateBins(logData);

            var section = new HazardDistributionSection() {
                CEDDistributionBins = bins,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
                Reference = new ReferenceDoseRecord(),
            };

            var chart = new HazardDistributionChartCreator(section, "mg/kg ");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        ///  Creates charts and test HazardDistributionSection view with uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var bins = simulateBins(logData);

            var section = new HazardDistributionSection() {
                CEDDistributionBins = bins,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true),
                Reference = new ReferenceDoseRecord(),
            };

            var chart = new HazardDistributionChartCreator(section, "mg/kg ");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}