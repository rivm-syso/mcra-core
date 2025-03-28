﻿using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryExposureBySubstanceBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly DietaryExposuresBySubstanceSection _section;
        private readonly string _unit;
        private readonly TargetLevelType _exposureLevel;

        public DietaryExposureBySubstanceBoxPlotChartCreator(DietaryExposuresBySubstanceSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.SubstanceBoxPlotRecords.Count * _cellSize, 80);
            _exposureLevel = section.TargetLevel;
            BoxColor = OxyColors.Green;
            StrokeColor = OxyColors.DarkGreen;
        }

        public override string ChartId {
            get {
                var pictureId = "7fadbced-d6a2-4e9b-a64a-e59724bddea5";
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

        private PlotModel create(ICollection<DietaryExposureBySubstancePercentileRecord> records, string unit) {
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
            var xOrder = 0;
            foreach (var item in recordsReversed) {
                categoryAxis.Labels.Add(item.SubstanceName);
                var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                var percentiles = item.Percentiles.Where(c => c > 0).ToList();
                whiskers = whiskers.Select(w => w == 0 ? double.NaN : w).ToArray();
                minimum = percentiles.Any() ? percentiles.Min() : 0;
                var boxPlotItem = setSeries(whiskers, null, xOrder, minimum, minimum, false);
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
