using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotHeatMapCreator : OxyPlotChartCreator {

        protected LinearAxis createHorizontalLinearAxis(double xLow, double xHigh) {
            var horizontalAxis = new LinearAxis() {
                Minimum = xLow,
                Maximum = xHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
            };
            return horizontalAxis;
        }

        protected LinearAxis createVerticalLinearAxis(double yLow, double yHigh) {
            var verticalAxis = new LinearAxis() {
                Minimum = yLow,
                Maximum = yHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Left,
            };
            return verticalAxis;
        }

        protected LogarithmicAxis createHorizontalLogarithmicAxis(double xLow, double xHigh) {
            var horizontalAxis = new LogarithmicAxis() {
                Minimum = xLow,
                Maximum = xHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Base = 10,
                Position = AxisPosition.Bottom,
            };
            return horizontalAxis;
        }

        protected LogarithmicAxis createVerticalLogarithmicAxis(double yLow, double yHigh) {
            var verticalAxis = new LogarithmicAxis() {
                Minimum = yLow,
                Maximum = yHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Base = 10,
                Position = AxisPosition.Left,
            };
            return verticalAxis;
        }
    }
}
