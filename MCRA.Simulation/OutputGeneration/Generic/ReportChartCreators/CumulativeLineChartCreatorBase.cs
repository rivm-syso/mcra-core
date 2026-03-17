using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CumulativeLineChartCreatorBase : ReportLineChartCreatorBase {

        protected PlotModel createPlotModel(
            UncertainDataPointCollection<double> percentiles,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit,
            string xtitle
        ) {
            var plotModel = createDefaultPlotModel();
            var allUncertaintyValues = percentiles.SelectMany(c => c.UncertainValues).Where(c => c > 0);
            var maximumUncertainty = allUncertaintyValues.Any() ? allUncertaintyValues.Max() : 0;
            var maximum = percentiles.Any() ? percentiles.Max(c => c.ReferenceValue) : 0;
            maximum = Math.Max(maximumUncertainty, maximum);

            var minimum = percentiles.Any(c => c.XValue > 0.1 && c.ReferenceValue > 0)
                ? percentiles.Where(c => c.XValue > 0.1 && c.ReferenceValue > 0).Min(c => c.ReferenceValue)
                : 1e-3;
            var horizontalAxis = createLogarithmicAxis(xtitle, minimum, maximum);
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("Probability", 0, 1);
            plotModel.Axes.Add(verticalAxis);

            var referenceLineSeries = new LineSeries() {
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 0.8
            };
            referenceLineSeries.Points.AddRange(percentiles
                .Select(r => new DataPoint(r.ReferenceValue, r.XValue / 100D)));
            plotModel.Series.Add(referenceLineSeries);

            if (percentiles.Any() && percentiles.First().UncertainValues.Any()) {
                var areaSeries = new AreaSeries() {
                    Color = OxyColors.Red,
                    Color2 = OxyColors.Red,
                    Fill = OxyColor.FromAColor(50, OxyColors.Red),
                    StrokeThickness = .5,
                };
                areaSeries.Points.AddRange(percentiles
                    .Select(r => new DataPoint(r.Percentile(uncertaintyLowerLimit), r.XValue / 100D)));
                //Add extra datapoint to get correct uncertainty area in plot (aligned to right vertical axes)
                areaSeries.Points.Add(new DataPoint(maximum, percentiles.XValues.Last() / 100d));
                areaSeries.Points2.AddRange(percentiles
                    .Select(r => new DataPoint(r.Percentile(uncertaintyUpperLimit), r.XValue / 100D)));

                plotModel.Series.Add(areaSeries);
            }
            return plotModel;
        }

        protected PlotModel createPlotModel(
            List<(string level, UncertainDataPointCollection<double> percentiles)> stratifiedPercentiles,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit,
            string xtitle
        ) {
            var plotModel = createDefaultPlotModel();
            var legend = new Legend() {
                LegendPlacement = LegendPlacement.Inside,
                LegendTitle = new string(' ', 20),
                LegendBackground = OxyColors.Transparent,
                LegendBorder = OxyColors.Transparent,
            };
            plotModel.Legends.Add(legend);
            plotModel.IsLegendVisible = true;
            var minimum = double.MaxValue;
            var maximum = double.MinValue;
            var nColors = stratifiedPercentiles.Count == 1 ? 2 : stratifiedPercentiles.Count;
            var palette = CustomPalettes.DietaryNonDietaryColors(nColors);
            var counter = 0;
            foreach (var group in stratifiedPercentiles) {
                var allUncertaintyValues = group.percentiles.SelectMany(c => c.UncertainValues).Where(c => c > 0);
                var maximumUncertainty = allUncertaintyValues.Any() ? allUncertaintyValues.Max() : 0;
                maximum = Math.Max(maximum, Math.Max(maximumUncertainty, group.percentiles.Any() ? group.percentiles.Max(c => c.ReferenceValue) : 0));
                minimum = Math.Min(minimum, Math.Min(minimum, group.percentiles.Any(c => c.XValue > 0.1 && c.ReferenceValue > 0)
                    ? group.percentiles.Where(c => c.XValue > 0.1 && c.ReferenceValue > 0).Min(c => c.ReferenceValue)
                    : 1e-3));
                var referenceLineSeries = new LineSeries() {
                    Color = palette.Colors[counter],
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 1.5,
                    Title = group.level
                };
                referenceLineSeries.Points.AddRange(group.percentiles
                    .Select(r => new DataPoint(r.ReferenceValue, r.XValue / 100D)));
                plotModel.Series.Add(referenceLineSeries);

                if (group.percentiles.Any() && group.percentiles.First().UncertainValues.Any()) {
                    var areaSeries = new AreaSeries() {
                        Color = palette.Colors[counter],
                        Color2 = palette.Colors[counter],
                        Fill = OxyColor.FromAColor(50, palette.Colors[counter]),
                        StrokeThickness = .5,
                    };
                    areaSeries.Points.AddRange(group.percentiles
                        .Select(r => new DataPoint(r.Percentile(uncertaintyLowerLimit), r.XValue / 100D)));
                    //Add extra datapoint to get correct uncertainty area in plot (aligned to right vertical axes)
                    areaSeries.Points.Add(new DataPoint(maximum, group.percentiles.XValues.Last() / 100d));
                    areaSeries.Points2.AddRange(group.percentiles
                        .Select(r => new DataPoint(r.Percentile(uncertaintyUpperLimit), r.XValue / 100D)));

                    plotModel.Series.Add(areaSeries);
                }
                counter++;
            }
            var horizontalAxis = createLogarithmicAxis(xtitle, minimum, maximum);
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("Probability", 0, 1);
            plotModel.Axes.Add(verticalAxis);
            return plotModel;
        }
    }
}
