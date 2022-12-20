using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class ConfidenceIntervalBarSeries : TornadoBarSeries {

        public ConfidenceIntervalBarSeries() {
            ErrorWidth = 0.3;
            ErrorStrokeThickness = 2;
            StrokeColor = OxyColors.Black;
            MarkerStrokeThickness = 1;
            MarkerType = MarkerType.Cross;
            MarkerSize = 4;
            RenderBaseValue = true;
            MarkerStroke = OxyColor.FromRgb(0, 0, 0);
            MarkerFill = OxyColor.FromRgb(0, 0, 0);
        }

        public bool RenderBaseValue { get; set; }

        public double ErrorStrokeThickness { get; set; }

        public double MarkerStrokeThickness { get; set; }

        public double ErrorWidth { get; set; }

        public MarkerType MarkerType { get; set; }

        public double MarkerSize { get; set; }

        public OxyColor MarkerStroke { get; set; }

        public OxyColor MarkerFill { get; set; }

        public override void Render(IRenderContext rc) {
            ActualMinimumBarRectangles = new List<OxyRect>();
            ActualMaximumBarRectangles = new List<OxyRect>();

            if (Items.Count == 0) {
                return;
            }

            var clippingRect = GetClippingRect();
            var categoryAxis = GetCategoryAxis();
            var actualBarWidth = GetActualBarWidth();

            for (var i = 0; i < Items.Count; i++) {
                var item = Items[i];

                var categoryIndex = item.CategoryIndex;

                var baseValue = double.IsNaN(item.BaseValue) ? BaseValue : item.BaseValue;
                var expectedValuePoint = Transform(baseValue, categoryIndex + categoryAxis.GetCurrentBarOffset(categoryIndex));

                ScreenPoint lowerConfidencePoint;
                if (!double.IsNaN(item.Minimum)) {
                    lowerConfidencePoint = Transform(item.Minimum, categoryIndex + categoryAxis.GetCurrentBarOffset(categoryIndex));
                } else {
                    lowerConfidencePoint = expectedValuePoint;
                }

                ScreenPoint upperConfidencePoint;
                if (!double.IsNaN(item.Maximum)) {
                    upperConfidencePoint = Transform(item.Maximum, categoryIndex + categoryAxis.GetCurrentBarOffset(categoryIndex));
                } else {
                    upperConfidencePoint = expectedValuePoint;
                }

                if (RenderBaseValue) {
                    // Draw marker
                    rc.DrawMarker(
                        clippingRect,
                        expectedValuePoint,
                        MarkerType,
                        null,
                        MarkerSize,
                        MarkerFill,
                        MarkerStroke,
                        MarkerStrokeThickness);
                }

                // Confidence interval
                if (!double.IsNaN(item.Minimum) || !double.IsNaN(item.Maximum)) {
                    rc.DrawClippedLine(
                        clippingRect,
                        new List<ScreenPoint> { lowerConfidencePoint, upperConfidencePoint },
                        0,
                        StrokeColor,
                        StrokeThickness,
                        LineStyle.Solid.GetDashArray(),
                        LineJoin.Miter,
                        true);

                    var leftValue = categoryIndex + categoryAxis.GetCurrentBarOffset(categoryIndex) + ((ErrorWidth / 2) * actualBarWidth);
                    var rightValue = categoryIndex + categoryAxis.GetCurrentBarOffset(categoryIndex) - ((ErrorWidth / 2) * actualBarWidth);
                    if (ErrorWidth > 0 && !double.IsNaN(item.Minimum)) {
                        var lowerLeftErrorPoint = Transform(item.Minimum, leftValue);
                        var lowerRightErrorPoint = Transform(item.Minimum, rightValue);
                        rc.DrawClippedLine(
                            clippingRect,
                            new List<ScreenPoint> { lowerLeftErrorPoint, lowerRightErrorPoint },
                            0,
                            StrokeColor,
                            ErrorStrokeThickness,
                            LineStyle.Solid.GetDashArray(),
                            LineJoin.Miter,
                            true);
                    }
                    if (ErrorWidth > 0 && !double.IsNaN(item.Maximum)) {
                        var upperLeftErrorPoint = Transform(item.Maximum, leftValue);
                        var upperRightErrorPoint = Transform(item.Maximum, rightValue);
                        rc.DrawClippedLine(
                            clippingRect,
                            new List<ScreenPoint> { upperLeftErrorPoint, upperRightErrorPoint },
                            0,
                            StrokeColor,
                            ErrorStrokeThickness,
                            LineStyle.Solid.GetDashArray(),
                            LineJoin.Miter,
                            true);
                    }
                }
            }
        }

        /// <summary>
        /// Renders the legend symbol on the specified rendering context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        /// <param name="legendBox">The legend rectangle.</param>
        public override void RenderLegend(IRenderContext rc, OxyRect legendBox) {
            double xmid = (legendBox.Left + legendBox.Right) / 2;
            double ymid = (legendBox.Top + legendBox.Bottom) / 2;
            var midpt = new ScreenPoint(xmid, ymid);
            var size = MarkerSize;
            rc.DrawEllipse(new OxyRect(midpt.X - size, midpt.Y - size, size * 2, size * 2), MarkerFill, MarkerStroke, MarkerStrokeThickness);
        }
    }
}
