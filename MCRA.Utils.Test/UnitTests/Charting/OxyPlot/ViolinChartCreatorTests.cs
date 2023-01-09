using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.Charting;
using MCRA.Utils.R.REngines;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class ViolinChartCreatorTests : ChartCreatorTestsBase {

        [TestMethod]
        public void HistogramChartCreator_TestCreateLinear() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var data = NormalDistribution.Samples(random, 0, 1, 100);
            var chartCreator = new HistogramChartCreator(
                data,
                title: "Histogram",
                titleX: "Linear horizontal axis",
                titleY: "Frequency"
            );
            WritePng(chartCreator, $"LinearHorizontalAxis.png");
        }


        /// <summary>
        /// Violin
        /// </summary>
        [TestMethod]
        public void ViolinChart_Test() {
            var data = Mock(100, 200, 300, 400, 500);
            var chart = new ViolinCreator(data, "violinplot");
            WritePng(chart, $"violinplot1");
        }

        /// <summary>
        /// Mock
        /// </summary>
        /// <param name="numberOfSamples"></param>
        /// <returns></returns>
        public IDictionary<string, List<double>> Mock(params int[] numberOfSamples) {
            var mu = 10;
            var sigma = 2;
            var result = new Dictionary<string, List<double>>();
            var counter = 0;
            foreach (var count in numberOfSamples) {
                var draw = NormalDistribution.NormalSamples(count, mu + counter * 3, sigma).ToList();
                result[$"{counter}"] = draw;
                counter++;  
            }
            return result;
        }
    }
}