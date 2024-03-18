using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
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
        [DataRow(true)]
        [DataRow(false)]
        public void HbmDataBoxPlotChartCreator_TestCreate(bool showOutliers) {
            var mu = 10;
            var sigma = .2;
            var nominalSize = 100;
            var seed = 1;
            var biologicalMatrix = BiologicalMatrix.Blood;
            var sampleTypeCode = "Whole blood";
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
                    Percentiles = percentiles.ToList(),
                    BiologicalMatrix = biologicalMatrix.GetDisplayName(),
                    SampleTypeCode = sampleTypeCode
                });
            }
            var section = new HbmSamplesBySamplingMethodSubstanceSection();
            var humanMonitoringSamplingMethod = new HumanMonitoringSamplingMethod { BiologicalMatrix = biologicalMatrix, SampleTypeCode = sampleTypeCode };
            section.HbmPercentilesRecords = new SerializableDictionary<HumanMonitoringSamplingMethod, List<HbmSampleConcentrationPercentilesRecord>>();
            section.HbmPercentilesRecords[humanMonitoringSamplingMethod] = hbmResults;
            var chart = new HbmDataBoxPlotChartCreator(section, humanMonitoringSamplingMethod, showOutliers);
            chart.CreateToPng(TestUtilities.ConcatWithOutputPath(
                showOutliers ? $"TestCreate_Outliers" : "TestCreate_NoOutliers"
            ));
        }
    }
}
