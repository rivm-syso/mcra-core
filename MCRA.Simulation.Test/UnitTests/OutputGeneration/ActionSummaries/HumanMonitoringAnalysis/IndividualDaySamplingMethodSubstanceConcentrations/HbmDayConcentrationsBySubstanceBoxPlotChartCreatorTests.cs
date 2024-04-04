using MCRA.General;
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
    public class HbmDayConcentrationsBySubstanceBoxPlotChartCreatorTests {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void HbmDayConcentrationsBySubstanceBoxPlotChartCreator_TestCreate() {
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
            var target = new ExposureTarget(BiologicalMatrix.Blood);
            var section = new HbmIndividualDayDistributionBySubstanceSection {
                HbmBoxPlotRecords = new() { { target, hbmResults } }
            };
            var chart = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(section.HbmBoxPlotRecords[target], target, string.Empty, string.Empty, false);
            chart.CreateToPng(TestUtilities.ConcatWithOutputPath($"TestCreate.png"));
        }
    }
}
