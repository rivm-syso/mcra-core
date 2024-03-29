﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TargetExposurePercentilesBySubstanceBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly TargetExposuresBySubstanceSection _section;
        private readonly string _unit;
        private readonly TargetLevelType _exposureLevel;
        private const int _cellSize = 20;

        public OxyColor BoxColor { get; set; } = OxyColors.Green;
        public OxyColor StrokeColor { get; set; } = OxyColors.DarkGreen;

        public TargetExposurePercentilesBySubstanceBoxPlotChartCreator(TargetExposuresBySubstanceSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.SubstanceBoxPlotRecords.Count * _cellSize, 80);
            _exposureLevel = section.TargetLevel;
        }

        public override string ChartId {
            get {
                var pictureId = "21c0166a-9342-4694-890a-e0f59ef91865";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override PlotModel Create() {
            var xtitle = $"Exposure ({_unit})";
            if (_exposureLevel == TargetLevelType.Internal) {
                xtitle = $"Concentration ({_unit})";
            }
            return create(_section.SubstanceBoxPlotRecords, xtitle);
        }

        private PlotModel create(ICollection<SubstanceTargetExposurePercentilesRecord> records, string unit) {
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse();
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
            var counter = 0;
            foreach (var item in recordsReversed) {
                categoryAxis.Labels.Add(item.SubstanceName);
                var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Any() ? percentiles.Min() : 0;
                var boxPlotItem = new BoxPlotItem(
                    counter,
                    double.IsNaN(item.P10) ? replace : item.P10,
                    double.IsNaN(item.P25) ? replace : item.P25,
                    double.IsNaN(item.P50) ? replace : item.P50,
                    double.IsNaN(item.P75) ? replace : item.P75,
                    double.IsNaN(item.P90) ? replace : item.P90
                );
                var boxPlotItem1 = new MultipleWhiskerBoxPlotItem(
                    boxPlotItem,
                    double.IsNaN(item.P5) ? replace : item.P5,
                    double.IsNaN(item.P95) ? replace : item.P95
                );

                series.Items.Add(boxPlotItem1);
                maximum = Math.Max(maximum, double.IsNaN(item.P95) ? maximum : item.P95);
                counter++;
            };
            logarithmicAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            logarithmicAxis.MajorStep = logarithmicAxis.MajorStep > 0 ? logarithmicAxis.MajorStep : double.NaN;
            logarithmicAxis.Minimum = minimum * .9;
            logarithmicAxis.AbsoluteMinimum = minimum * .9;
            plotModel.Axes.Add(logarithmicAxis);
            plotModel.Axes.Add(categoryAxis);
            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}
