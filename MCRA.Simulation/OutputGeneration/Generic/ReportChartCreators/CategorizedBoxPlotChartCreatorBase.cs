using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class CategorizedBoxPlotChartCreatorBase : BoxPlotChartCreatorBase {

        protected PlotModel create(
           ICollection<(string stratifier, BoxPlotChartRecord bps)> records,
           string labelHorizontalAxis,
           bool showOutliers,
           bool showLabels = true,
           int outliersLimit = 100
       ) {
            var plotModel = createDefaultPlotModel();

            var recordsReversed = records.Where(c => c.bps.Percentage > 0).Reverse();
            var minima = records.Where(r => r.bps.MinPositives > 0).Select(r => r.bps.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var stratificationGroups = recordsReversed.GroupBy(r => r.stratifier).ToList();
            var descriptorValues = recordsReversed.Select(c => c.bps.GetLabel()).ToHashSet();
            var descriptorIndexes = descriptorValues
                .Select((r, ix) => (r, ix))
                .ToDictionary(r => r.r, r => r.ix);

            var palette = CustomPalettes.DistinctTone(stratificationGroups.Count);
            var counter = 0;

            var maximum = double.NegativeInfinity;
            foreach (var group in stratificationGroups) {

                // Create boxplot series
                var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                    Title = group.Key,
                    Fill = OxyColor.FromAColor(200, palette.Colors[counter]),
                    StrokeThickness = .50,
                    Stroke = OxyColors.Black,
                    BoxWidth = 0.8,
                    WhiskerWidth = 1.1,

                };
                foreach (var (stratifier, bps) in group) {
                    var ix = descriptorIndexes[bps.GetLabel()];
                    var outliers = new List<double>();
                    if (showOutliers) {
                        if (bps.Outliers?.Count > outliersLimit) {
                            // Restrict the number of outliers to specified limit
                            outliers = [.. bps.Outliers.Take(outliersLimit - 2)];
                            // Including the minimum and maximum of the outliers
                            outliers.Add(bps.Outliers.Min());
                            outliers.Add(bps.Outliers.Max());
                        } else {
                            outliers = bps.Outliers;
                        }
                    }
                    var whiskers = getWhiskers(bps.P5, bps.P10, bps.P25, bps.P50, bps.P75, bps.P90, bps.P95);
                    var percentiles = bps.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                    var replace = percentiles.Any() ? percentiles.Min() : 0;

                    var boxPlotItem = createBoxPlotItem(whiskers, outliers, ix, replace, 0, showOutliers);
                    series.Items.Add(boxPlotItem);
                    maximum = Math.Max(maximum, double.IsNaN(bps.P95) ? maximum : bps.P95);
                }
                counter++;
                plotModel.Series.Add(series);
            }

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                MajorStep = 1,
                Position = AxisPosition.Left,
                IsTickCentered = false,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColors.Black,
                GapWidth = 0.1
            };
            var categories = recordsReversed.Select(c => c.bps.GetLabel()).ToHashSet();
            foreach (var category in categories) {
                categoryAxis.Labels.Add(category);
            }
            plotModel.Axes.Add(categoryAxis);

            // Create horizontal logarithmic axis
            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = labelHorizontalAxis,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
                MajorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 2,
            };
            plotModel.Axes.Add(logarithmicAxis);
            updateLogarithmicAxis(logarithmicAxis, minimum, maximum, 1.1);

            var legend = new PositionLegend {
                LegendBackground = OxyColors.Transparent,
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
            };
            plotModel.Legends.Add(legend);
            plotModel.IsLegendVisible = true;
            return plotModel;
        }

        internal class PositionLegend : Legend {
            public double ExtraWidthPadding { get; set; } = 20;

            public override OxySize GetLegendSize(IRenderContext rc, OxySize s) {
                setDefaults();
                OxySize baseSize = base.GetLegendSize(rc, s);

                if (baseSize.Width <= 0) {
                    return baseSize;
                }
                var newWidth = baseSize.Width + ExtraWidthPadding;
                return new OxySize(newWidth, baseSize.Height);
            }
            private void setDefaults() {
                this.LegendFontSize = 13;
                this.LegendLineSpacing = 4;
            }

            /// <summary>
            /// Get position of legend, use defaults or shift to the left to avoid cutting off legend text. 
            /// The shift is applied by multiplying the left position by a factor (e.g., 0.94) to move it slightly to the left.
            /// </summary>
            public override OxyRect GetLegendRectangle(OxySize legendSize) {
                var shift = 0.94;
                var p = base.GetLegendRectangle(legendSize);
                var position = new OxyRect(p.Left * shift, p.Top, p.Width, p.Height);
                return position;
            }
        }
    }
}
