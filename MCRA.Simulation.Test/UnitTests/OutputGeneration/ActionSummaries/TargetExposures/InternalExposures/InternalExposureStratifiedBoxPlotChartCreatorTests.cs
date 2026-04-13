using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, boxplot by source, stratified and original
    /// </summary>
    [TestClass]
    public class InternalExposureStratifiedBoxPlotChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        public void InternalExposureStratifiedBoxPlotChartCreator_TestUnstratified() {
            var seed = 1;
            var numPercentiles = 7;
            var random = new McraRandomGenerator(seed);
            var sources = Enum.GetValues<ExposureSource>()
                .Cast<ExposureSource>()
                .Where(r => r != ExposureSource.Undefined)
                .ToList();
            var records = new List<ExposureBySourceBoxPlotRecord>();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (var i = 0; i < sources.Count; i++) {
                var percentiles = new List<double>();
                for (int j = 0; j < numPercentiles; j++) {
                    percentiles.Add(random.NextDouble() * 10 + i * 10);
                }
                var record = new ExposureBySourceBoxPlotRecord {
                    Source = sources[i / 2].GetDisplayName(),
                    Percentiles = [.. percentiles.OrderBy(c => c)],
                    MinPositives = percentiles.Min() * 0.5,
                    MaxPositives = percentiles.Max() * 2,
                    Percentage = 100
                };
                if (i != 1) {
                    records.Add(record);
                }
            }
            var section = new ExposureBySourceSection { BoxPlotRecords = records };
            var chart = new InternalExposureBoxPlotChartCreator<SourceContributorKey, ExposureBySourceBoxPlotRecord>("Source", records, targetUnit, false);
            RenderChart(chart, $"TestUnstratified");
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        public void InternalExposureStratifiedBoxPlotChartCreator_TestStratified(int numLevels) {
            var seed = 1;
            var numPercentiles = 7;
            var random = new McraRandomGenerator(seed);
            var sources = Enum.GetValues<ExposureSource>()
                .Cast<ExposureSource>()
                .Where(r => r != ExposureSource.Undefined)
                .ToList();
            var stratification = Enumerable.Range(1, numLevels).Select(r => $"Group {r}").ToArray();
            var records = new List<ExposureBySourceBoxPlotRecord>();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (var i = 0; i < sources.Count * stratification.Length; i++) {
                var percentiles = new List<double>();
                for (int j = 0; j < numPercentiles; j++) {
                    percentiles.Add(random.NextDouble() * 10 + i / stratification.Length * 10);
                }
                var record = new ExposureBySourceBoxPlotRecord {
                    Source = sources[i / stratification.Length].GetDisplayName(),
                    Stratification = stratification[i % stratification.Length],
                    Percentiles = [.. percentiles.OrderBy(c => c)],
                    MinPositives = percentiles.Min() * 0.5,
                    MaxPositives = percentiles.Max() * 2,
                    Percentage = 100,
                    Outliers = [.. Enumerable.Range(0, 10).Select(r => random.NextDouble(.95, 2) * percentiles.Max())]
                };
                if (i != 1) {
                    records.Add(record);
                }
            }
            var section = new ExposureBySourceSection { BoxPlotRecords = records };
            var chart = new InternalExposureStratifiedBoxPlotChartCreator<SourceContributorKey, ExposureBySourceBoxPlotRecord>("Source", records, targetUnit, true, false);
            RenderChart(chart, $"TestStratified_{numLevels}");
        }
    }
}
