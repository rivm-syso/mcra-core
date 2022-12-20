using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration, Generic, Diagnostics
    /// </summary>
    [TestClass]
    public class HbmDayConcentrationsBySubstanceBoxPlotChartCreatorTests {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void Hbm_ChartTest2() {
            var mu = 10;
            var sigma = .2;
            var nominalSize = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var hbmResults = new List<HbmConcentrationsPercentilesRecord>();
            for (int i = 0; i < 2; i++) {
                var percentiles = NormalDistribution.Samples(random, mu + i * .2, sigma, nominalSize).Percentiles(percentages);
                hbmResults.Add(new HbmConcentrationsPercentilesRecord() {
                    SubstanceCode = $"-{i}",
                    SubstanceName = $"substance-{i}",
                    Description = $"AM-{i}",
                    Percentiles = percentiles.ToList()
                });
            }

            var section = new HbmIndividualDayDistributionBySubstanceSection();
            section.HbmBoxPlotRecords = hbmResults;
            var chart = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(section, "");
            chart.CreateToPng(TestResourceUtilities.ConcatWithOutputPath($"_HBM data Multiple2.png"));
        }
    }
}
