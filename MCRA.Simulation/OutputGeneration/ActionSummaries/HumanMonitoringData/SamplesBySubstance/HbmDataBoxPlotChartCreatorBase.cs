﻿using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmDataBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected readonly int _cellSize = 20;
        protected string _concentrationUnit;

        public OxyColor BoxColor { get; set; } = OxyColors.CornflowerBlue;
        public OxyColor StrokeColor { get; set; } = OxyColors.Blue;

        protected PlotModel create(
            ICollection<HbmSampleConcentrationPercentilesRecord> records,
            string unit,
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
                var (maximum, minimum) = setSeries(records, categoryAxis, series);
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
                var (maximum, minimum) = setSeries(records, categoryAxis, series);
                logarithmicAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
                logarithmicAxis.MajorStep = logarithmicAxis.MajorStep > 0 ? logarithmicAxis.MajorStep : double.NaN;
                logarithmicAxis.Minimum = minimum * .9;
                logarithmicAxis.AbsoluteMinimum = minimum * .9;
                plotModel.Axes.Add(logarithmicAxis);
            }
            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }

        private static (double, double) setSeries(
            ICollection<HbmSampleConcentrationPercentilesRecord> records,
            CategoryAxis categoryAxis,
            MultipleWhiskerHorizontalBoxPlotSeries series
        ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse().ToList();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;
            var isMultipleSampleTypes = records.Select(r => r.SampleTypeCode).Distinct().Count() > 1;
            var isMultipleMatrices = records.Select(r => r.BiologicalMatrix).Distinct().Count() > 1;
            var maximum = double.NegativeInfinity;
            var counter = 0;
            foreach (var item in recordsReversed) {
                var label = item.SubstanceName;
                if (isMultipleMatrices && isMultipleMatrices) {
                    label += $" ({item.BiologicalMatrix} - {item.SampleTypeCode})";
                } else if (isMultipleMatrices) {
                    label += $" ({item.BiologicalMatrix})";
                } else if (isMultipleSampleTypes) {
                    label += $" ({item.SampleTypeCode})";
                }
                categoryAxis.Labels.Add(label);
                var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Any() ? percentiles.Min() : item.LOR;
                var boxPlotItem = new BoxPlotItem(
                    counter,
                    double.IsNaN(item.P10) ? replace : item.P10,
                    double.IsNaN(item.P25) ? replace : item.P25,
                    double.IsNaN(item.P50) ? replace : item.P50,
                    double.IsNaN(item.P75) ? replace : item.P75,
                    double.IsNaN(item.P90) ? replace : item.P90
                );

                //Restrict the number of outliers to a maximum of 100 + 2, especially useful for acute assessments
                var outliers = item.Outliers.Count > 100 ? item.Outliers.Take(100).ToList() : item.Outliers;
                if (item.Outliers.Count > 100) {
                    outliers.Add(item.Outliers.Min());
                    outliers.Add(item.Outliers.Max());
                }
                boxPlotItem.Outliers = outliers;
                var boxPlotItem1 = new MultipleWhiskerBoxPlotItem(
                    boxPlotItem,
                    double.IsNaN(item.P5) ? replace : item.P5,
                    double.IsNaN(item.P95) ? replace : item.P95,
                    item.LOR
                );

                series.Items.Add(boxPlotItem1);
                minimum = item.LOR > 0 ? Math.Min(minimum, item.LOR) : minimum;
                maximum = Math.Max(maximum, double.IsNaN(item.P95) ? item.LOR : item.P95);
                counter++;
            };
            return (maximum, minimum);
        }
    }
}
