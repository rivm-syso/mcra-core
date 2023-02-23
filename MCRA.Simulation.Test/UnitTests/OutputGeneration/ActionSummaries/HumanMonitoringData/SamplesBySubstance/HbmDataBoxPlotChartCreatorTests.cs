using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration, Generic, Diagnostics
    /// </summary>
    [TestClass]
    public class HbmDataBoxPlotChartCreatorTests {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void HbmDataBoxPlotChartCreator_TestCreate() {
            var mu = 10;
            var sigma = .2;
            var nominalSize = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var hbmResults = new List<HbmSampleConcentrationPercentilesRecord>();
            for (int i = 0; i < 2; i++) {
                var percentiles = NormalDistribution.Samples(random, mu + i * .2, sigma, nominalSize).Percentiles(percentages);
                hbmResults.Add(new HbmSampleConcentrationPercentilesRecord() {
                    SubstanceCode = $"-{i}",
                    SubstanceName = $"substance-{i}",
                    Description = $"AM-{i}",
                    LOR = (percentiles[0] + percentiles[1]) / 2,
                    Percentiles = percentiles.ToList()
                });
            }
            var section = new HbmSamplesBySamplingMethodSubstanceSection();
            section.HbmPercentilesRecords = hbmResults;
            var chart = new HbmDataBoxPlotChartCreator(section, "");
            chart.CreateToPng(TestUtilities.ConcatWithOutputPath($"_HBM data Multiple1.png"));
        }
    }
}
