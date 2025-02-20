using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmDataBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected string _concentrationUnit;

        protected PlotModel create(
            ICollection<HbmSampleConcentrationPercentilesRecord> records,
            string unit,
            bool showOutliers,
            bool isLinearAxis = false
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
                setSeries(records, categoryAxis, series, showOutliers);
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
                var (minimum, maximum) = setSeries(records, categoryAxis, series, showOutliers);
                updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
                plotModel.Axes.Add(logarithmicAxis);
            }
            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }

        private (double, double) setSeries(
            ICollection<HbmSampleConcentrationPercentilesRecord> records,
            CategoryAxis categoryAxis,
            MultipleWhiskerHorizontalBoxPlotSeries series,
            bool showOutliers
        ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse().ToList();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;
            var isMultipleSampleTypes = records.Select(r => r.SampleTypeCode).Distinct().Count() > 1;
            var isMultipleMatrices = records.Select(r => r.BiologicalMatrix).Distinct().Count() > 1;
            var maximum = double.NegativeInfinity;
            var xOrder = 0;
            foreach (var record in recordsReversed) {
                var label = record.SubstanceName;
                if (isMultipleMatrices && isMultipleMatrices) {
                    label += $" ({record.BiologicalMatrix} - {record.SampleTypeCode})";
                } else if (isMultipleMatrices) {
                    label += $" ({record.BiologicalMatrix})";
                } else if (isMultipleSampleTypes) {
                    label += $" ({record.SampleTypeCode})";
                }
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
                var replace = percentiles.Any() ? percentiles.Min() : record.LOR;
                var boxPlotItem = setSeries(whiskers, outliers, xOrder, replace, record.LOR, showOutliers);
                series.Items.Add(boxPlotItem);
                minimum = record.LOR > 0 ? Math.Min(minimum, record.LOR) : minimum;
                maximum = Math.Max(maximum, double.IsNaN(record.P95) ? record.LOR : record.P95);
                xOrder++;
            };
            return (minimum, maximum);
        }
    }
}
