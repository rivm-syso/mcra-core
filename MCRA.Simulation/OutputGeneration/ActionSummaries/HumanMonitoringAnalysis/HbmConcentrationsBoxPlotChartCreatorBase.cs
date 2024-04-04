using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmConcentrationsBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected const int _cellSize = 20;

        public OxyColor BoxColor { get; set; } = OxyColors.CornflowerBlue;
        public OxyColor StrokeColor { get; set; } = OxyColors.Blue;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        protected PlotModel create(
            ICollection<HbmConcentrationsPercentilesRecord> records,
            string unit,
            bool showOutLiers,
            bool showLabels = true
        ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var plotModel = createDefaultPlotModel();
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

            var maximum = double.NegativeInfinity;
            var xOrder = 0;
            foreach (var item in recordsReversed) {
                if (showLabels) {
                    categoryAxis.Labels.Add(item.Description);
                }
                var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Any() ? percentiles.Min() : 0;
                var boxPlotItem = setSeries(whiskers, item.Outliers, xOrder, replace, 0, showOutLiers);
                series.Items.Add(boxPlotItem);
                maximum = Math.Max(maximum, double.IsNaN(item.P95) ? maximum : item.P95);
                xOrder++;
            };
            updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
            plotModel.Axes.Add(logarithmicAxis);
            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}


