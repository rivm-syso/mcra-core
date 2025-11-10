using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CombinedDietaryExposuresChartCreator : ReportChartCreatorBase {

        private readonly CombinedDietaryExposurePercentilesSection _section;
        private readonly double _percentile;
        private readonly OxyPalette _palette;

        public CombinedDietaryExposuresChartCreator(
            CombinedDietaryExposurePercentilesSection section,
            double percentile
        ) {
            Width = 700;
            Height = 150 + section.ModelSummaryRecords.Count * 18;
            _section = section;
            _percentile = percentile;
            _palette = CustomPalettes.Monochrome(_section.Percentages.Count, 0.5883, .2, .5, 1, 1, false);
        }

        public override string ChartId {
            get {
                var pictureId = "fe10ad79-6a6a-4b91-bd2a-8e05d7a2c120";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title {
            get {
                return !double.IsNaN(_percentile)
                    ? $"Exposure percentiles with uncertainty bound for P{_percentile}"
                    : "Exposure percentiles";
            }
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel();

            var sortPercentage = _percentile;
            var models = _section.ModelSummaryRecords
                .OrderBy(r => _section.GetPercentileRecord(r.Id, sortPercentage)?.Value ?? double.NaN)
                .ThenByDescending(r => r.Name)
                .ToList();

            plotModel.IsLegendVisible = true;
            var Legend = new Legend {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop
            };
            plotModel.Legends.Add(Legend);

            var xAxis = new LogarithmicAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1,
                Title = $"Exposure [{_section.ExposureUnit.GetShortDisplayName()}]",
                Position = AxisPosition.Bottom,
                UseSuperExponentialFormat = true
            };
            plotModel.Axes.Add(xAxis);

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                Position = AxisPosition.Left,
            };
            for (int i = 0; i < models.Count; i++) {
                categoryAxis.Labels.Add(models[i].Name);
            }
            plotModel.Axes.Add(categoryAxis);

            var idx = 0;
            foreach (var percentile in _section.Percentages) {
                var items = models
                    .Select(r => _section.GetPercentileRecord(r.Id, percentile))
                    .ToList();
                if (items.Any(r => r.HasUncertainty)) {
                    var series = new ConfidenceIntervalBarSeries() {
                        Title = $"p{percentile}",
                        MarkerSize = 4,
                        MarkerStroke = _palette.Colors[idx],
                        MarkerFill = _palette.Colors[idx],
                        MarkerType = MarkerType.Circle,
                    };
                    for (int i = 0; i < items.Count; i++) {
                        var r = items[i];
                        if (r != null) {
                            series.Items.Add(new TornadoBarItem() {
                                BaseValue = r?.UncertaintyMedian ?? r.Value,
                                Minimum = r?.UncertaintyLowerBound ?? double.NaN,
                                Maximum = r?.UncertaintyUpperBound ?? double.NaN,
                                CategoryIndex = i,
                            });
                        }
                    }
                    plotModel.Series.Add(series);
                } else {
                    var series = new ScatterSeries() {
                        //LabelFormatString = "{Value:00.###}",
                        Title = $"p{percentile}",
                        MarkerSize = 4,
                        MarkerStroke = _palette.Colors[idx],
                        MarkerFill = _palette.Colors[idx],
                        MarkerType = MarkerType.Circle,
                    };
                    var points = new List<ScatterPoint>();
                    for (int i = 0; i < items.Count; i++) {
                        var r = items[i];
                        if (r != null) {
                            points.Add(new ScatterPoint(r.Value, i, double.NaN, percentile));
                        }
                    }
                    series.ItemsSource = points;
                    plotModel.Series.Add(series);
                }
                idx++;
            }

            return plotModel;
        }
    }
}
