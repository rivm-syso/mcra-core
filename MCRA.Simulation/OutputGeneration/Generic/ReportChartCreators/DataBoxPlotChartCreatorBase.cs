using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class DataBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected string _concentrationUnit;

        protected PlotModel create(
            ICollection<PercentilesRecord> records,
            string unit
        ) {
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
            var (minimum, maximum) = setSeries(records, categoryAxis, series, true);
            updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
            plotModel.Axes.Add(logarithmicAxis);

            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }

        private (double, double) setSeries(
            ICollection<PercentilesRecord> records,
            CategoryAxis categoryAxis,
            MultipleWhiskerHorizontalBoxPlotSeries series,
            bool showOutliers
        ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse().ToList();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;
            var maximum = double.NegativeInfinity;
            var xOrder = 0;
            foreach (var record in recordsReversed) {
                var label = record.SubstanceName;
                categoryAxis.Labels.Add(label);

                //Restrict the number of outliers to a maximum of 100 + 2, especially useful for acute assessments
                var outliers = showOutliers
                    ? (record.Outliers.Count > 100 ? record.Outliers.Take(100).ToList() : record.Outliers)
                    : null;
                if (outliers != null && record.Outliers.Count > 100) {
                    outliers.Add(record.Outliers.Min());
                    outliers.Add(record.Outliers.Max());
                }
                var whiskers = getWhiskers(record.P5, record.P10, record.P25, record.P50, record.P75, record.P90, record.P95);
                var percentiles = record.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Any() ? percentiles.Min() : double.NaN;
                var boxPlotItem = setSeries(whiskers, outliers, xOrder, replace, 0, showOutliers);
                series.Items.Add(boxPlotItem);
                minimum = Math.Min(minimum, percentiles.Min());
                maximum = Math.Max(maximum, double.IsNaN(record.P95) ? percentiles.Min() : record.P95);
                xOrder++;
            };
            return (minimum, maximum);
        }
    }
}
