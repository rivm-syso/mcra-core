using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmVsModelledBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        public OxyColor ModelledBoxColor { get; set; } = OxyColors.Green;
        public OxyColor ModelledStrokeColor { get; set; } = OxyColors.DarkGreen;
        public OxyColor MonitoringBoxColor { get; set; } = OxyColors.CornflowerBlue;
        public OxyColor MonitoringStrokeColor { get; set; } = OxyColors.Blue;

        protected PlotModel create(
            ICollection<BiologicalMatrixConcentrationPercentilesRecord> records,
            string unit,
            bool cumulative = false
        ) {
            var minima = records
                .Where(r => r.MinPositives > 0)
                .Select(r => r.MinPositives)
                .ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var recordsReversed = records
                .OrderByDescending(r => r.SubstanceName)
                .ThenBy(r => r.Type == "Modelled")
                .ToList();
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

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                Position = AxisPosition.Left
            };
            var seriesMonitoring = new MultipleWhiskerHorizontalBoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, ModelledBoxColor),
                StrokeThickness = 1,
                Stroke = ModelledStrokeColor,
                BoxWidth = .4,
                WhiskerWidth = 1.1,
                Title = "Blue: monitoring"
            };
            var seriesModelled = new MultipleWhiskerHorizontalBoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, MonitoringBoxColor),
                StrokeThickness = 1,
                Stroke = MonitoringStrokeColor,
                BoxWidth = .4,
                WhiskerWidth = 1.1,
                Title = "Green: modelled"
            };
            var maximum = double.NegativeInfinity;
            var xOrder = 0;
            foreach (var item in recordsReversed) {
                var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                var boxPlotItem = createBoxPlotItem(whiskers, null, xOrder, 0, 0, false);
                if (xOrder % 2 == 1) {
                    if (cumulative) {
                        categoryAxis1.Labels.Add($"Modelled");
                    } else {
                        categoryAxis1.Labels.Add(item.SubstanceName);
                    }
                    seriesMonitoring.Items.Add(boxPlotItem);
                } else {
                    if (cumulative) {
                        categoryAxis1.Labels.Add($"Monitoring");
                    } else {
                        categoryAxis1.Labels.Add($"-");
                    }
                    seriesModelled.Items.Add(boxPlotItem);
                }
                maximum = Math.Max(maximum, item.P95);
                xOrder++;
            };
            logarithmicAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            logarithmicAxis.MajorStep = logarithmicAxis.MajorStep > 0 ? logarithmicAxis.MajorStep : double.NaN;
            logarithmicAxis.Minimum = minimum;
            logarithmicAxis.AbsoluteMinimum = minimum;
            plotModel.Axes.Add(logarithmicAxis);
            plotModel.Axes.Add(categoryAxis1);

            plotModel.Series.Add(seriesModelled);
            plotModel.Series.Add(seriesMonitoring);

            return plotModel;
        }
    }
}
