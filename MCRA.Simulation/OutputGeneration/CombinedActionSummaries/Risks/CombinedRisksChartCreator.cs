using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CombinedRisksChartCreator : OxyPlotChartCreator {

        private readonly CombinedRiskPercentilesSection _section;
        private readonly double _percentile;
        private string _riskType;
        private readonly OxyPalette _palette;

        public CombinedRisksChartCreator(
            CombinedRiskPercentilesSection section,
            double percentile
        ) {
            Width = 700;
            Height = 100 + section.ExposureModelSummaryRecords.Count * 18;
            _section = section;
            _percentile = percentile;
            _palette = CustomPalettes.Monochrome(_section.Percentages.Count, 0.5883, .2, .5, 1, 1, false);
            _riskType = section.RiskMetric.GetDisplayName();
        }

        public override string ChartId {
            get {
                var pictureId = "39dc3d6e-e2d0-4d10-8bf1-e11f2625a1a0";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title => !double.IsNaN(_percentile) 
            ? $"{_riskType} percentiles with uncertainty bound for P{_percentile:F2}" 
            : $"{_riskType} percentiles";

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel();
            var sortPercentage = _percentile;
            var models = _section.ExposureModelSummaryRecords
                .OrderBy(r => _section.GetPercentile(r.Id, sortPercentage)?.Risk ?? double.NaN)
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
                Title = _riskType,
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
                    .Select(r => _section.GetPercentile(r.Id, percentile))
                    .ToList();
                if (items.Any(r => r.HasUncertainty())) {
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
                                BaseValue = r?.UncertaintyMedian ?? r.Risk,
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
                            points.Add(new ScatterPoint(r.Risk, i, double.NaN, percentile));
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
