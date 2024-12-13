using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class CustomPieSeries : PieSeries {

        /// <summary>
        /// This string must be empty otherwise no legenda is made
        /// </summary>
        private readonly string _addLegenda = "  ";

        public CustomPieSeries() {
            Diameter = .8;
            ExplodedDistance = .05;
            OutsideLabelFormat = " ";
            InsideLabelPosition = 0.85;
            InsideLabelColor = OxyColors.White;
            InsideLabelFormat = "{2:F1}%";
            TickRadialLength = 0;
            TickHorizontalLength = 0;
            Height = 350;
            Width = 500;
            Title = _addLegenda;
            FontWeight = FontWeights.Bold;
        }

        public int Height { get; set; }

        public int Width { get; set; }

        public OxyPalette Pallete { get; set; }

        public double OuterRadius {
            get {
                var radius = Math.Min(PlotModel.PlotArea.Width, PlotModel.PlotArea.Height) / 2;
                return radius * (Diameter - ExplodedDistance);
            }
        }

        public double InnerRadius {
            get {
                var radius = Math.Min(PlotModel.PlotArea.Width, PlotModel.PlotArea.Height) / 2;
                return radius * InnerDiameter;
            }
        }

        public ScreenPoint MidPoint {
            get {
                return new ScreenPoint((PlotModel.PlotArea.Left + PlotModel.PlotArea.Right) * 0.25, (PlotModel.PlotArea.Top + PlotModel.PlotArea.Bottom) * 0.5);
            }
        }

        /// <summary>
        /// Implements <see cref="PieSeries.Render(IRenderContext)"/>.
        /// </summary>
        /// <param name="rc"></param>
        public override void Render(IRenderContext rc) {
            if (!Slices.Any()) {
                RenderErrorMessage(rc, "Error: The data does not contain any items.");
                return;
            }
            if (Slices.Any(r => double.IsNaN(r.Value))) {
                RenderErrorMessage(rc, "Error: The data contains items with unknown contributions.");
                return;
            }
            var total = Slices.Sum(slice => slice.Value);
            if (Math.Abs(total) < double.Epsilon) {
                return;
            }

            var radius = Math.Min(PlotModel.PlotArea.Width, PlotModel.PlotArea.Height) / 2;

            var outerRadius = OuterRadius;
            var innerRadius = InnerRadius;

            var angle = StartAngle;
            var midPoint = MidPoint;

            foreach (var slice in Slices) {
                var outerPoints = new List<ScreenPoint>();
                var innerPoints = new List<ScreenPoint>();

                var sliceAngle = slice.Value / total * AngleSpan;
                var endAngle = angle + sliceAngle;
                var explodedRadius = slice.IsExploded ? ExplodedDistance * radius : 0.0;

                var midAngle = angle + (sliceAngle / 2);
                var midAngleRadians = midAngle * Math.PI / 180;
                var mp = new ScreenPoint(
                    midPoint.X + (explodedRadius * Math.Cos(midAngleRadians)),
                    midPoint.Y + (explodedRadius * Math.Sin(midAngleRadians)));

                // Create the pie sector points for both outside and inside arcs
                // while (true) { //this has caused out of memory exceptions!
                // use maximum of 10000 iterations, more than 10000 outer points on a piechart
                // is probably a mistake
                for (int i = 0; i < 10000; i++) {
                    var stop = false;
                    if (angle >= endAngle) {
                        angle = endAngle;
                        stop = true;
                    }

                    var a = angle * Math.PI / 180;
                    var op = new ScreenPoint(mp.X + (outerRadius * Math.Cos(a)), mp.Y + (outerRadius * Math.Sin(a)));
                    outerPoints.Add(op);
                    var ip = new ScreenPoint(mp.X + (innerRadius * Math.Cos(a)), mp.Y + (innerRadius * Math.Sin(a)));
                    if (innerRadius + explodedRadius > 0) {
                        innerPoints.Add(ip);
                    }

                    if (stop) {
                        break;
                    }

                    angle += AngleIncrement;
                }

                innerPoints.Reverse();
                if (innerPoints.Count == 0) {
                    innerPoints.Add(mp);
                }

                innerPoints.Add(outerPoints[0]);

                var points = outerPoints;
                points.AddRange(innerPoints);

                rc.DrawPolygon(points, slice.ActualFillColor, Stroke, StrokeThickness, EdgeRenderingMode.Automatic, null, LineJoin.Bevel);

                // Render label outside the slice
                if (OutsideLabelFormat != null) {
                    var label = string.Format(OutsideLabelFormat, slice.Value, slice.Label, slice.Value / total * 100);
                    var sign = Math.Sign(Math.Cos(midAngleRadians));

                    // tick points
                    var tp0 = new ScreenPoint(
                        mp.X + ((outerRadius + TickDistance) * Math.Cos(midAngleRadians)),
                        mp.Y + ((outerRadius + TickDistance) * Math.Sin(midAngleRadians)));
                    var tp1 = new ScreenPoint(
                        tp0.X + (TickRadialLength * Math.Cos(midAngleRadians)),
                        tp0.Y + (TickRadialLength * Math.Sin(midAngleRadians)));
                    var tp2 = new ScreenPoint(tp1.X + (TickHorizontalLength * sign), tp1.Y);

                    // draw the tick line with the same color as the text
                    rc.DrawLine([tp0, tp1, tp2], ActualTextColor, 1, EdgeRenderingMode.Automatic, null, LineJoin.Bevel);

                    // label
                    var labelPosition = new ScreenPoint(tp2.X + (TickLabelDistance * sign), tp2.Y);
                    rc.DrawText(
                        labelPosition,
                        label,
                        ActualTextColor,
                        ActualFont,
                        ActualFontSize,
                        ActualFontWeight,
                        0,
                        sign > 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                        VerticalAlignment.Middle);
                }

                // Render a label inside the slice
                if (InsideLabelFormat != null && !InsideLabelColor.IsUndefined() && slice.Value / total * 100 > 1) {
                    var label = string.Format(InsideLabelFormat, slice.Value, slice.Label, slice.Value / total * 100);
                    var r = (innerRadius * (1 - InsideLabelPosition)) + (outerRadius * InsideLabelPosition);
                    var labelPosition = new ScreenPoint(mp.X + (r * Math.Cos(midAngleRadians)), mp.Y + (r * Math.Sin(midAngleRadians)));
                    var textAngle = 0d;
                    if (AreInsideLabelsAngled) {
                        textAngle = midAngle;
                        if (Math.Cos(midAngleRadians) < 0) {
                            textAngle += 180;
                        }
                    }

                    var actualInsideLabelColor = InsideLabelColor.IsAutomatic() ? ActualTextColor : InsideLabelColor;

                    rc.DrawText(
                        labelPosition,
                        label,
                        actualInsideLabelColor,
                        ActualFont,
                        ActualFontSize,
                        ActualFontWeight,
                        textAngle,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Middle);
                }
            }
        }

        /// <summary>
        /// Renders the specified error message.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        /// <param name="errorMessage">The error message.</param>
        private void RenderErrorMessage(IRenderContext rc, string errorMessage) {
            var p0 = new ScreenPoint(PlotModel.PlotArea.Left, PlotModel.PlotArea.Top);
            rc.DrawMultilineText(
                p0,
                errorMessage,
                OxyColors.Red,
                ActualFont,
                ActualFontSize,
                ActualFontWeight
            );
        }
    }
}
