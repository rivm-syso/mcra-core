using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class StratifiedBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected PlotModel create(
            ICollection<(string stratifier, BoxPlotChartRecord bps)> records,
            string labelHorizontalAxis,
            bool showOutliers,
            bool showLabels = true,
            int outliersLimit = 100
        ) {
            var recordsReversed = records.Where(c => c.bps.Percentage > 0).Reverse();
            var minima = records.Where(r => r.bps.MinPositives > 0).Select(r => r.bps.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var legend = new Legend {
                LegendBackground = OxyColor.FromArgb(200, 255, 255, 255),
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendFontSize = 13
            };
            var plotModel = createDefaultPlotModel();
            plotModel.Legends.Add(legend);
            plotModel.IsLegendVisible = true;
            var groups = recordsReversed.GroupBy(r => r.stratifier).ToList();

            var palette = CustomPalettes.DistinctTone(groups.Count);
            var counter = 0;
            var maximum = double.NegativeInfinity;
            foreach (var group in groups) {
                // Create boxplot series
                var xOrder = 0;
                var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                    Fill = OxyColor.FromAColor(200, palette.Colors[counter]),
                    StrokeThickness = .75,
                    //Stroke = palette.Colors[counter],
                    Stroke = OxyColors.Black,
                    BoxWidth = .4,
                    WhiskerWidth = 1.1,
                };
                var fakeLegenda = new ScatterSeries() {
                    MarkerFill = OxyColor.FromAColor(200, palette.Colors[counter]),
                    Title = group.Key
                };
                foreach (var gr in group) {
                    var item = gr.bps;
                    var ix = (groups.Count * xOrder) + counter;
                    var outliers = new List<double>();
                    if (showOutliers) {
                        if (item.Outliers?.Count > outliersLimit) {
                            // Restrict the number of outliers to specified limit
                            outliers = [.. item.Outliers.Take(outliersLimit - 2)];
                            // Including the minimum and maximum of the outliers
                            outliers.Add(item.Outliers.Min());
                            outliers.Add(item.Outliers.Max());
                        } else {
                            outliers = item.Outliers;
                        }
                    }

                    var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                    var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                    var replace = percentiles.Any() ? percentiles.Min() : 0;
                    var boxPlotItem = createBoxPlotItem(whiskers, outliers, ix, replace, 0, showOutliers);
                    series.Items.Add(boxPlotItem);
                    maximum = Math.Max(maximum, double.IsNaN(item.P95) ? maximum : item.P95);
                    xOrder++;
                }
                counter++;
                plotModel.Series.Add(series);
                plotModel.Series.Add(fakeLegenda);
            }

            // Create horizontal logarithmic axis
            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = labelHorizontalAxis,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
                MajorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 2
            };

            updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
            plotModel.Axes.Add(logarithmicAxis);
            var linearAxis = new LinearAxis() {
                Position = AxisPosition.Right,
                MajorStep = 1,
                IsAxisVisible = false,
            };

            // Oxyplot renders labels on the major step of the category axis, so we need to set the major step
            // to the number of groups to ensure that labels are rendered correctly
            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                MajorStep = 1,
                //MajorStep = groups.Count,
                Position = AxisPosition.Left,
                IsTickCentered = false,
                MajorGridlineStyle = LineStyle.Dot,
            };

            if (showLabels) {
                foreach (var item in recordsReversed) {
                    var label = getLabel(item.bps.GetLabel(), item.stratifier);
                    categoryAxis.Labels.Add(label);
                }
            }
            plotModel.Axes.Add(categoryAxis);
            //Add linear axis after adding the category axis to ensure it is rendered on top of the category axis
            plotModel.Axes.Add(linearAxis);

            return plotModel;
        }

        public static string getLabel(string label, string stratifier) {
            if (label.Contains(stratifier)) {
                int start;
                start = label.IndexOf(stratifier, 0);
                return label.Substring(0, start - 2);
            }
            return label;
        }
    }
}
