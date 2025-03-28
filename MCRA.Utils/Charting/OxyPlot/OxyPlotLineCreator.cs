using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotLineCreator : OxyPlotChartCreator {

        protected LineSeries createDefaultLineSeries() {
            var lineSeries = new LineSeries() {
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Black,
                StrokeThickness = 0.8,
            };
            return lineSeries;
        }

        protected LinearAxis createDefaultBottomLinearAxis() {
            var linearAxis = new LinearAxis() {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
            };
            return linearAxis;
        }

        /// <summary>
        /// Linear axis,
        /// Position = AxisPosition.Left,
        /// MajorGridlineStyle = LineStyle.Dash,
        /// MinorGridlineStyle = LineStyle.Dash,
        /// MinorTickSize = 4,
        /// </summary>
        /// <param name="maximum"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        protected static LinearAxis createLinearAxis(
            string title,
            double minimum = double.NaN,
            double maximum = double.NaN,
            AxisPosition position = AxisPosition.Left
        ) {
            return new LinearAxis() {
                Position = position,
                Title = title,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dash,
                MinorTickSize = 4,
                Minimum = minimum,
                Maximum = maximum,
            };
        }

        /// <summary>
        /// Logarithmic axis
        /// Position = AxisPosition.Bottom,
        /// MajorGridlineStyle = LineStyle.Dash,
        /// MinorGridlineStyle = LineStyle.None,
        /// MinorTickSize = 0,
        /// </summary>
        /// <returns></returns>
        protected LogarithmicAxis createLogarithmicAxis(string title, double minimum, double maximum) {
            var axis = createLogarithmicAxis(title);
            if (minimum > 0) {
                axis.Minimum = minimum;
                axis.Maximum = maximum;
            }
            return axis;
        }

        protected LogarithmicAxis createLogarithmicAxis(
            string title,
            AxisPosition position = AxisPosition.Bottom
        ) {
            return new LogarithmicAxis() {
                Position = position,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Title = title,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
            };
        }
    }
}