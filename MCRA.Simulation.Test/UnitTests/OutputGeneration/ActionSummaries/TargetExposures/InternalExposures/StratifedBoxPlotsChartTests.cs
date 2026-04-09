using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, boxplot by source, stratified and original
    /// </summary>
    [TestClass]
    public class StratifiedBoxPlotsChartTests : ChartCreatorTestBase {

        /// <summary>

        [TestMethod]
        public void StratifiedBoxPlotsChartTests_TestOriginalStratified() {
            var seed = 1;
            var number = 7;
            var random = new McraRandomGenerator(seed);
            var sources = new[] { "Oral", "Dermal", "Inhalation" };
            var stratification = new[] { "Male", "Female" };
            var records = new List<ExposureBySourceBoxPlotRecord>();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (var i = 0; i < 6; i++) {
                var percentiles = new List<double>();
                for (int j = 0; j < number; j++) {
                    percentiles.Add(random.NextDouble() * 10 + i);
                }
                var record = new ExposureBySourceBoxPlotRecord {
                    Source = sources[i / 2],
                    Stratification = stratification[i % 2],
                    Percentiles = [.. percentiles.OrderBy(c => c)],
                    MinPositives = percentiles.Min() * 0.5,
                    MaxPositives = percentiles.Max() * 2,
                    Percentage = 100
                };
                records.Add(record);
            }
            var section = new ExposureBySourceSection { BoxPlotRecords = records };
            var chart = new InternalExposureBoxPlotChartCreator<SourceContributorKey, ExposureBySourceBoxPlotRecord>("Source", records, targetUnit, false, false);
            RenderChart(chart, $"OriginalStratifiedBoxPlotsChartTest");
        }

        [TestMethod]
        public void StratifiedBoxPlotsChartTests_TestStratified() {
            var seed = 1;
            var number = 7;
            var random = new McraRandomGenerator(seed);
            var sources = new[] { "Oral", "Dermal", "Inhalation" };
            var stratification = new[] { "Male", "Female" };
            var records = new List<ExposureBySourceBoxPlotRecord>();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (var i = 0; i < 6; i++) {
                var percentiles = new List<double>();
                for (int j = 0; j < number; j++) {
                    percentiles.Add(random.NextDouble() * 10 + i / 2 * 10);
                }
                var record = new ExposureBySourceBoxPlotRecord {
                    Source = sources[i / 2],
                    Stratification = stratification[i % 2],
                    Percentiles = [.. percentiles.OrderBy(c => c)],
                    MinPositives = percentiles.Min() * 0.5,
                    MaxPositives = percentiles.Max() * 2,
                    Percentage = 100
                };
                records.Add(record);
            }
            var section = new ExposureBySourceSection { BoxPlotRecords = records };
            var chart = new InternalExposureStratifiedBoxPlotChartCreator<SourceContributorKey, ExposureBySourceBoxPlotRecord>("Source", records, targetUnit, false, false);
            RenderChart(chart, $"StratifiedBoxPlotsChartTest");
        }
    }
}
