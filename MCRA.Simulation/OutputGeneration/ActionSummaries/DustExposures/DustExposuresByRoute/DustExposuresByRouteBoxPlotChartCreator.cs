using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustExposuresByRouteBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<DustExposuresPercentilesRecord> _records;
        private readonly ExposureRoute _route;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public DustExposuresByRouteBoxPlotChartCreator(
            List<DustExposuresPercentilesRecord> records,
            ExposureRoute route,
            string sectionId,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _route = route;
            _sectionId = sectionId;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "68f6d4e5-6076-4d3d-9b55-8d5b67d38fd0";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _route.GetHashCode());
            }
        }

        public override PlotModel Create() {
            var recordsReversed = _records.Where(c => c.Percentage > 0).Reverse();
            var minima = _records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = (double)(minima.Count != 0 ? minima.Min() * 0.9 : 1e-8);

            var plotModel = createDefaultPlotModel();
            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = $"Exposure ({_unit})",
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
                categoryAxis.Labels.Add(item.GetLabel());
                var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Count != 0 ? percentiles.Min() : 0;
                var boxPlotItem = createBoxPlotItem(whiskers, item.Outliers, xOrder, replace, 0, _showOutliers);
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
