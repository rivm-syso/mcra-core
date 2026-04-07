using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public abstract class OxyPlotStackedBarChartCreator : OxyPlotChartCreator {
        protected PlotModel create(
            List<BarDataPoint> data,
            OxyPalette palette = null
        ) {

            var stratifiers = data.Select(d => d.Category).ToHashSet();
            var plotModel = createDefaultPlotModel(string.Empty);
            plotModel.IsLegendVisible = true;

            var categoryAxis = new CategoryAxis() {
                Key = "y",
                MinorStep = 1,
                Angle = -15
            };
            foreach (var stratifier in stratifiers) {
                categoryAxis.Labels.Add($"{stratifier}");
            }
            plotModel.Axes.Add(categoryAxis);

            var linearAxis = new LinearAxis() {
                AbsoluteMinimum = 0,
                Key = "x",
                MaximumPadding = 0.06,
                MinimumPadding = 0
            };
            plotModel.Axes.Add(linearAxis);
            var groups = data.GroupBy(c => c.Serie).ToList();
            var allCategories = data.Select(d => d.Category).OrderBy(c => c).ToHashSet();
            if (palette == null) {
                palette = CustomPalettes.SplitComplementary(groups.Count(), 0.5883, .3, .3, .9, .9);
            }
            plotModel.DefaultColors = [.. palette.Colors.Select(c => OxyColor.FromAColor(175, c))];
            foreach (var group in groups) {
                var barSeries = new BarSeries {
                    IsStacked = true,
                    StrokeThickness = 0.1,
                    XAxisKey = "x",
                    YAxisKey = "y",
                    Title = group.Key,
                    LabelFormatString = "{0:0.0}%",
                    LabelPlacement = LabelPlacement.Middle,
                    TextColor = OxyColors.White,
                };
                foreach (var category in allCategories) {
                    var item = group.FirstOrDefault(g => g.Category == category);
                    var barItem = item != null ? new BarItem(item.Contribution, -1) : new BarItem(0, -1);
                    barSeries.Items.Add(barItem);
                }
                plotModel.Series.Add(barSeries);
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

