using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public abstract class OxyPlotStackedBarChartWithErrorsCreator : OxyPlotChartCreator {
        protected PlotModel create(
            List<StackedColumnWithErrorItem> data,
            OxyPalette palette = null,
            bool uncertainty = false
        ) {
            var categories = data.GroupBy(d => d.Label).ToList();
            var subCategories = data
                .SelectMany(d => d.Subcategories.Keys)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var plotModel = createDefaultPlotModel(string.Empty);
            plotModel.IsLegendVisible = true;

            var categoryAxis = new CategoryAxis() {
                Key = "y",
                MinorStep = 1,
                Position = AxisPosition.Left
            };
            foreach (var category in categories) {
                categoryAxis.Labels.Add($"{category.Key}");
            }
            plotModel.Axes.Add(categoryAxis);

            var linearAxis = new LinearAxis() {
                AbsoluteMinimum = 0,
                Key = "x",
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Position = AxisPosition.Bottom
            };
            plotModel.Axes.Add(linearAxis);

            if (palette == null) {
                palette = CustomPalettes.BeachTone(subCategories.Count);
            }
            plotModel.DefaultColors = [.. palette.Colors.Select(c => OxyColor.FromAColor(175, c))];

            foreach (var subCategory in subCategories) {
                var barSeries = new BarSeries {
                    IsStacked = true,
                    StrokeThickness = 0.2,
                    XAxisKey = "x",
                    YAxisKey = "y",
                    Title = subCategory
                };
                var categoryIndex = 0;
                foreach (var category in categories) {
                    var item = category.Single();
                    if (item.Subcategories.TryGetValue(subCategory, out var value)) {
                        barSeries.Items.Add(new BarItem(value, categoryIndex));
                    }
                    categoryIndex++;
                }
                plotModel.Series.Add(barSeries);
            }

            if (uncertainty) {
                var errorBarSeries = new LineSeries {
                    XAxisKey = "x",
                    YAxisKey = "y",
                    Color = OxyColors.Black,
                    StrokeThickness = 1.5,
                    LineStyle = LineStyle.Solid,
                    RenderInLegend = false
                };
                const double cap = 0.08;

                var index = 0;
                foreach (var category in categories) {
                    var item = category.Single();
                    var lower = item.Lower;
                    var upper = item.Upper;

                    errorBarSeries.Points.Add(new DataPoint(lower, index));
                    errorBarSeries.Points.Add(new DataPoint(upper, index));
                    errorBarSeries.Points.Add(DataPoint.Undefined);
                    errorBarSeries.Points.Add(new DataPoint(lower, index - cap));
                    errorBarSeries.Points.Add(new DataPoint(lower, index + cap));
                    errorBarSeries.Points.Add(DataPoint.Undefined);
                    errorBarSeries.Points.Add(new DataPoint(upper, index - cap));
                    errorBarSeries.Points.Add(new DataPoint(upper, index + cap));
                    errorBarSeries.Points.Add(DataPoint.Undefined);

                    index++;
                }
                plotModel.Series.Add(errorBarSeries);
            }

            var legend = new Legend {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
            };
            plotModel.Legends.Add(legend);

            return plotModel;
        }
    }
}

