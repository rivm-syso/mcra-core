using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CumulativeLineChartCreatorBase : OxyPlotLineCreator {

        protected PlotModel createPlotModel(
            UncertainDataPointCollection<double> percentiles,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit,
            string xtitle
        ) {
            var plotModel = createDefaultPlotModel();
            var minimum = percentiles.Any() ? percentiles.Min(c => c.ReferenceValue) : 0;
            var maximum = percentiles.Any() ? percentiles.Max(c => c.ReferenceValue) : 0;
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
                areaSeries.Points2.AddRange(percentiles
                    .Select(r => new DataPoint(r.Percentile(uncertaintyUpperLimit), r.XValue / 100D)));

                plotModel.Series.Add(areaSeries);
            }
            return plotModel;
        }
    }
}
