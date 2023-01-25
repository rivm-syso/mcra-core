using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Utils.Charting.OxyPlot {

    /// <summary>
    /// Represents a linear color axis.
    /// </summary>
    public class NonDetectBarsAxis : LinearAxis {

        //public class NonDetectBarsAxis : LinearAxis, IColorAxis {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearColorAxis" /> class.
        /// </summary>
        public NonDetectBarsAxis() {
            Position = AxisPosition.Left;
            TickStyle = TickStyle.None;
            MajorTickSize = 20;
            LabelFormatter = d => { return string.Empty; };
            Color = OxyColors.Red;
            Height = 120;
            Rescale = 1;
        }

        private readonly double PositionTierMinShift = 0;

        private readonly double scaling = 70;

        /// <summary>
        /// Height of bar as percen
        /// </summary>
        public double Fraction { get ; set;}

        /// <summary>
        /// adapt height of bar
        /// </summary>
        public double Rescale { get; set; }

        /// <summary>
        /// The color of the bar.
        /// </summary>
        public OxyColor Color { get; set; }

        /// <summary>
        /// The bar label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The bar-height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Renders the axis on the specified render context.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="pass">The render pass.</param>
        public override void Render(IRenderContext rc, int pass) {

            if (Position == AxisPosition.None) {
                return;
            }

            if (pass == 0) {
                double distance = AxisDistance;
                double left = PlotModel.PlotArea.Left;
                double width = MajorTickSize - 2;
                double height = MajorTickSize - 2;

                switch (Position) {
                    case AxisPosition.Left:
                        left = PlotModel.PlotArea.Left - PositionTierMinShift - width - distance;
                        break;
                    case AxisPosition.Right:
                        left = PlotModel.PlotArea.Right + PositionTierMinShift + distance;
                        break;
                    case AxisPosition.Top:
                        left = PlotModel.PlotArea.Left;
                        break;
                    case AxisPosition.Bottom:
                        left = PlotModel.PlotArea.Left;
                        break;
                }

                Action<double, double, OxyColor> drawColorRect = (ylow, yhigh, color) => {
                    double ymin = Math.Min(ylow, yhigh);
                    double ymax = Math.Max(ylow, yhigh) + 0.5;
                    rc.DrawRectangle(
                        new OxyRect(left, ymin, width, ymax - ymin),
                        color,
                        OxyColors.Undefined,
                        1,
                        EdgeRenderingMode.Automatic);
                };

                double yLow = Transform(0);
                double yHigh = Transform(Fraction * scaling * Rescale);

                drawColorRect(yLow, yHigh, Color);
                rc.DrawText(new ScreenPoint(left, Math.Min(yLow, yHigh) - 2), text: $"{Fraction * 100:F1} %", OxyColors.Black, Font, FontSize, FontWeight, 270);
                rc.DrawText(new ScreenPoint(left, Height), Label, OxyColors.Black, Font, FontSize, FontWeight, 270);
            }
            base.Render(rc, pass);
        }
    }
}
