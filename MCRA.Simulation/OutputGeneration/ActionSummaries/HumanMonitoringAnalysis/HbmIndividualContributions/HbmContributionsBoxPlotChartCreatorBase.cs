using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmContributionsBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected string _concentrationUnit;

        protected PlotModel create(
            ICollection<HbmContributionPercentilesRecord> records,
            string unit,
            bool showOutliers,
            bool isLinearAxis = false
         ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse().ToList();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var plotModel = createDefaultPlotModel();
            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                Position = AxisPosition.Left
            };
            var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, BoxColor),
                StrokeThickness = 1,
                Stroke = StrokeColor,
                BoxWidth = .4,
                WhiskerWidth = 1.1,
            };
            var isMultipleMatrices = records.Select(r => r.BiologicalMatrix).Distinct().Count() > 1;
            if (isLinearAxis) {
                var linearAxis = new LinearAxis() {
                    Position = AxisPosition.Bottom,
                    Title = unit,
                    Minimum = 0,
                    Maximum = 100,
                    MaximumPadding = 0.1,
                    MinimumPadding = 0.1,
                    MajorStep = 10,
                    MinorStep = 10,
                    MajorGridlineStyle = LineStyle.Dash,
                    MajorTickSize = 2
                };
                var xOrder = 0;
                foreach (var record in recordsReversed) {
                    var label = isMultipleMatrices ? $"{record.SubstanceName} ({record.BiologicalMatrix})" : record.SubstanceName;
                    categoryAxis.Labels.Add(label);
                    var whiskers = getWhiskers(record.P5, record.P10, record.P25, record.P50, record.P75, record.P90, record.P95);
                    var percentiles = record.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                    var replace = percentiles.Any() ? percentiles.Min() : minimum;
                    var boxPlotItem = createBoxPlotItem(whiskers, record.Outliers, xOrder, replace, double.NaN, showOutliers);
                    series.Items.Add(boxPlotItem);
                    xOrder++;
                }
                plotModel.Axes.Add(linearAxis);
            } else {
                var logarithmicAxis = new LogarithmicAxis() {
                    Position = AxisPosition.Bottom,
                    Title = unit,
                    MaximumPadding = 0.1,
                    MinimumPadding = 0.1,
                    MajorStep = 100,
                    MinorStep = 100,
                    MajorGridlineStyle = LineStyle.Dash,
                    MajorTickSize = 2
                };
                var xOrder = 0;
                foreach (var record in recordsReversed) {
                    var label = isMultipleMatrices ? $"{record.SubstanceName} ({record.BiologicalMatrix})" : record.SubstanceName;
                    categoryAxis.Labels.Add(label);
                    var whiskers = getWhiskers(record.P5, record.P10, record.P25, record.P50, record.P75, record.P90, record.P95);
                    var percentiles = record.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                    var replace = percentiles.Any() ? percentiles.Min() : minimum;
                    var boxPlotItem = createBoxPlotItem(whiskers, record.Outliers, xOrder, replace, double.NaN, showOutliers);
                    series.Items.Add(boxPlotItem);
                    xOrder++;
                }
                updateLogarithmicAxis(logarithmicAxis, minimum, double.NegativeInfinity);
                plotModel.Axes.Add(logarithmicAxis);
            }
            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}
