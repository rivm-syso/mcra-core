using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, DietaryExposureDistribution
    /// </summary>
    [TestClass]
    public class DietaryUpperIntakeDistributionChartTests : ChartCreatorTestBase {

        private int number = 5000;
        /// <summary>
        /// Summarize and test DietaryUpperIntakeDistributionSection view, create chart
        /// </summary>
        [TestMethod]
        public void DietaryTotalIntakeDistributionChart_Test1() {
            var logData = NormalDistribution.NormalSamples(number, .5, 1.5).OrderBy(c => c).Skip(number - 1000).ToList();
            var bins = simulateBins(logData);
            var section = new DietaryUpperIntakeDistributionSection() {
                IntakeDistributionBins = bins,
                TotalNumberOfIntakes = number
            };

            var chart = new DietaryUpperIntakeDistributionChartCreator(section, "mg/kg bw/day");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, data.Count).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
    }
}