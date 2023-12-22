using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Test.UnitTests.Helpers.ChartCreators {
    public sealed class ScatterChartCreator : OxyPlotLineCreator {
        private List<double> _y;
        private List<double> _x;

        public ScatterChartCreator(List<double> y, List<double> x) {
            Width = 500;
            Height = 350;
            _y = y;
            _x = x;
        }

        public override PlotModel Create() {
            return create(_y, _x);
        }

        private PlotModel create(List<double> y, List<double> x) {
            var plotModel = createDefaultPlotModel();

            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 1.5,
            };
            for (int i = 0; i < y.Count; i++) {
                series2.Points.Add(new ScatterPoint(y[i], x[i]));
            }

            plotModel.Series.Add(series2);

            var horizontalAxis = createLinearAxis("x");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("y");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
