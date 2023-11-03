using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentClusterBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly IndividualsExposureSection _section;
        private const int _cellSize = 20;

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
                        var percentiles = item.Percentiles.Where(c => c > 0).ToList();
                        var replace = percentiles.Any() ? percentiles.Min() : 0;
                        var positionY = (numberOfComponents - j) * numberOfClusters + i;
                        var boxPlotItem = new BoxPlotItem(
                            x: positionY,
                            item.P10 == 0 ? replace : item.P10,
                            item.P25 == 0 ? replace : item.P25,
                            item.P50 == 0 ? replace : item.P50,
                            item.P75 == 0 ? replace : item.P75,
                            item.P90 == 0 ? replace : item.P90
                        );
                        var boxPlotItem1 = new MultipleWhiskerBoxPlotItem(
                            boxPlotItem,
                            item.P5 == 0 ? replace : item.P5,
                            item.P95 == 0 ? replace : item.P95
                        );

                        series.Items.Add(boxPlotItem1);
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

            logarithmicAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            logarithmicAxis.MajorStep = logarithmicAxis.MajorStep > 0 ? logarithmicAxis.MajorStep : double.NaN;
            logarithmicAxis.Minimum = minimum * .9;
            logarithmicAxis.AbsoluteMinimum = minimum * .9;
            plotModel.Axes.Add(logarithmicAxis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}
