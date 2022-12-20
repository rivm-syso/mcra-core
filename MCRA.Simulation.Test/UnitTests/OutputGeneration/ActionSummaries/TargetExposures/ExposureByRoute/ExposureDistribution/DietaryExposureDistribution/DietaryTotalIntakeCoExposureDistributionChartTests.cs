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
    public class DietaryTotalIntakeCoExposureDistributionChartTests : ChartCreatorTestBase {

        private int number = 5000;
        /// <summary>
        /// Create chart, test DietaryTotalIntakeCoExposureDistributionSection view
        /// </summary>
        [TestMethod]
        public void DietaryTotalIntakeCoExposureDistributionChart_Test1() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var data = NormalDistribution.Samples(rnd, 5, 1.5, number);
            var referenceDose = Math.Pow(10, data.Percentile(82));
            var bins = simulateBins(data);
            var binsCoExposure = simulateBins(data.Skip(2000).ToList());
            var section = new DietaryTotalIntakeCoExposureDistributionSection() {
                IntakeDistributionBins = bins,
                IntakeDistributionBinsCoExposure = binsCoExposure,
                TotalNumberOfIntakes = number,
                PercentageZeroIntake = 0
            };
            var chart = new DietaryTotalIntakeCoExposureDistributionChartCreator(section, "mg/kg bw/day");
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