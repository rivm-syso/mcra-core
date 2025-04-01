using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentClusterBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly IndividualsExposureSection _section;

        public ComponentClusterBoxPlotChartCreator(IndividualsExposureSection section) {
            _section = section;
            Width = 500;
            Height = 80 + Math.Max(_section.BoxPlotSummaryRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "cf07c3f2-c22a-49a8-adda-870ddd8e9fd8";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override PlotModel Create() {
            var xtitle = $"Exposure";
            return create(_section.BoxPlotSummaryRecords, xtitle);
        }

        private PlotModel create(Dictionary<(int component, int cluster), ComponentClusterPercentilesRecord> records, string unit) {
            var minima = records.Values.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;
            var numberOfComponents = records.Keys.Max(c => c.component);
            var numberOfClusters = records.Keys.Max(c => c.cluster);
            var palette = CustomPalettes.DistinctTone(numberOfClusters);

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
                Position = AxisPosition.Left,
                TickStyle = TickStyle.None
            };

            var labelPosition = 0;
            var componentCounter = numberOfComponents;
            var maximum = double.NegativeInfinity;
            //Per cluster one serie, each cluster has its own color, each serie contains all components.
            //Plot in diagram per component, so each component contains all clusters
            //Plot is build up from bottom to top
            for (int i = 0; i < numberOfClusters; i++) {
                var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                    Fill = OxyColor.FromAColor(100, palette.Colors[i]),
                    StrokeThickness = 1,
                    Stroke = palette.Colors[i],
                    BoxWidth = .4,
                    WhiskerWidth = 1.1,
                    Title = $"Subgroup {i + 1}",
                };

                for (int j = numberOfComponents; j > 0; j--) {
                    if (records.TryGetValue((j, i + 1), out var item)) {
                        if (labelPosition % numberOfClusters == numberOfClusters - 1) {
                            categoryAxis.Labels.Add($"Comp: {componentCounter}");
                            componentCounter--;
                        } else {
                            categoryAxis.Labels.Add("");
                        }
                        item.Percentiles.ForEach(c => c = (c == 0 ? double.NaN : c));
                        var xOrder = (numberOfComponents - j) * numberOfClusters + i;
                        var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                        var percentiles = item.Percentiles.Where(c => c > 0).ToList();
                        var replace = percentiles.Any() ? percentiles.Min() : 0;
                        var boxPlotItem = createBoxPlotItem(whiskers, null, xOrder, replace, 0, false);
                        series.Items.Add(boxPlotItem);
                        maximum = Math.Max(maximum, item.P95 == 0 ? maximum : item.P95);
                    };
                    labelPosition++;
                }
                plotModel.Series.Add(series);
            };
            plotModel.IsLegendVisible = true;
            plotModel.Legends.Add(new CustomMultiWiskerLegend() {
                LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside,
                LegendPosition = OxyPlot.Legends.LegendPosition.RightTop
            });

            updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
            plotModel.Axes.Add(logarithmicAxis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}
