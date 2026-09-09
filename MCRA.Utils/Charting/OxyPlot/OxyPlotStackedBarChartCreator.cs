using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public abstract class OxyPlotStackedBarChartCreator : OxyPlotChartCreator {
        private readonly double _minimumContribution = 7;
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
                palette = CustomPalettes.DistinctTone(groups.Count);
            }
            var othersByCategory = new Dictionary<string, List<(double Contribution, BarSeries Series, int CategoryIndex)>>();
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
                    TextColor = OxyColors.Black,
                };
                var categoryIndex = 0;
                foreach (var category in allCategories) {
                    var item = group.FirstOrDefault(g => g.Category == category);
                    if (item != null) {
                        if (item.Contribution < _minimumContribution) {
                            if (!othersByCategory.ContainsKey(category)) {
                                othersByCategory[category] = new List<(double, BarSeries, int)>();
                            }
                            othersByCategory[category].Add((item.Contribution, barSeries, categoryIndex));

                        } else {
                            barSeries.Items.Add(new BarItem(item.Contribution, categoryIndex));
                        }
                    }
                    categoryIndex++;
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

            {
                //Add "Others" series if there are contributions below the minimum threshold (= 7%)
                if (othersByCategory.Count == 0) {
                    return plotModel;
                }
                var othersSeries = new BarSeries {
                    IsStacked = true,
                    StrokeThickness = 0.1,
                    XAxisKey = "x",
                    YAxisKey = "y",
                    Title = "Others",
                    LabelFormatString = "{0:0.0}%",
                    LabelPlacement = LabelPlacement.Middle,
                    TextColor = OxyColors.Black,
                    FillColor = OxyColors.LightGray,
                };

                var categoryIndex = 0;
                var hasOthersItems = false;
                foreach (var cat in allCategories) {
                    if (othersByCategory.TryGetValue(cat, out var contributions) && contributions.Count > 0) {
                        if (contributions.Count == 1) {
                            var singleContribution = contributions[0];
                            singleContribution.Series.Items.Add(new BarItem(singleContribution.Contribution, singleContribution.CategoryIndex));
                        } else {
                            var totalContribution = contributions.Sum(c => c.Contribution);
                            othersSeries.Items.Add(new BarItem(totalContribution, categoryIndex));
                            hasOthersItems = true;
                        }
                    }
                    categoryIndex++;
                }

                if (hasOthersItems) {
                    plotModel.Series.Add(othersSeries);
                }
            }


            return plotModel;
        }
    }
}

