using MCRA.Utils.Statistics;
using OxyPlot;
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

            var minimum = percentiles.Any() ? percentiles.Where(c => c.XValue > 0.1 && c.ReferenceValue > 0).Min(c => c.ReferenceValue) : 1e-3;
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
    }
}
