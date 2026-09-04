using OxyPlot;
using OxyPlot.Series;
using RDotNet;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CumulativeChartCreatorBase : ReportLineChartCreatorBase {

        protected PlotModel createPlotModel(
            List<(double x, double y)> data,
            List<(double x, double y)> fittedValues = null,
            string xTitle = "Exposure",
            string yTitle = "Cumulative probability"
        ) {
            var plotModel = createDefaultPlotModel();

            // Determine minimum and maximum1
            var minimum = data.Min(c => c.x);
            var maximum = data.Max(c => c.x);
            // Create axes
            var horizontalAxis = createLogarithmicAxis(xTitle, minimum, maximum);
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis(yTitle, 0, 1);
            plotModel.Axes.Add(verticalAxis);
            {
                var series = new ScatterSeries() {
                    MarkerSize = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColor.FromAColor(100, OxyColors.DarkGray),
                    MarkerStroke = OxyColors.Black,
                };
                for (int i = 0; i < data.Count; i++) {
                    series.Points.Add(new ScatterPoint(data[i].x, data[i].y / 100));
                }
                plotModel.Series.Add(series);
            }
            {
                if (fittedValues != null) {
                    var series = new LineSeries() {
                        Color = OxyColors.Red,
                        LineStyle = LineStyle.Automatic,
                        StrokeThickness = 1.5,
                    };
                    for (int i = 0; i < fittedValues.Count; i++) {
                        series.Points.Add(new DataPoint(fittedValues[i].x, fittedValues[i].y));
                    }
                    plotModel.Series.Add(series);
                }
            }
            return plotModel;
        }
    }
}
