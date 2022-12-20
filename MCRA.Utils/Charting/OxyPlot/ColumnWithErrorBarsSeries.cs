using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class ColumnWithErrorBarsSeries : ErrorColumnSeries {

        public ColumnWithErrorBarsSeries() {
        }

        /// <summary>
        /// Updates the max/minimum values.
        /// </summary>
        protected override void UpdateMaxMin() {
            base.UpdateAxisMaxMin();
            MinY = Items.Cast<ErrorColumnItem>().Select(r => !double.IsNaN(r.Error) ?  r.Value - r.Error : r.Value).Min();
            MaxY = Items.Cast<ErrorColumnItem>().Select(r => !double.IsNaN(r.Error) ? r.Value + r.Error : r.Value).Max();
        }

        protected override void RenderItem(IRenderContext rc, OxyRect clippingRect, double topValue, double categoryValue, double actualBarWidth, BarItemBase item, OxyRect rect) {
            var errorItem = item as ErrorColumnItem;
            if (errorItem == null) {
                return;
            }

            rc.DrawClippedRectangleAsPolygon(clippingRect, rect, GetSelectableFillColor(ActualFillColor), StrokeColor, StrokeThickness);

            // Render the error
            var lowerValue = topValue - errorItem.Error;
            var upperValue = topValue + errorItem.Error;
            var left = 0.5 - ErrorWidth / 2;
            var right = 0.5 + ErrorWidth / 2;
            var leftValue = categoryValue + (left * actualBarWidth);
            var middleValue = categoryValue + (0.5 * actualBarWidth);
            var rightValue = categoryValue + (right * actualBarWidth);

            var lowerErrorPoint = Transform(middleValue, lowerValue);
            var upperErrorPoint = Transform(middleValue, upperValue);
            rc.DrawClippedLine(
                clippingRect,
                new List<ScreenPoint> { lowerErrorPoint, upperErrorPoint },
                0,
                StrokeColor,
                ErrorStrokeThickness,
                LineStyle.Solid.GetDashArray(),
                LineJoin.Miter,
                true);

            if (ErrorWidth > 0) {
                var lowerLeftErrorPoint = Transform(leftValue, lowerValue);
                var lowerRightErrorPoint = Transform(rightValue, lowerValue);
                rc.DrawClippedLine(
                    clippingRect,
                    new List<ScreenPoint> { lowerLeftErrorPoint, lowerRightErrorPoint },
                    0,
                    StrokeColor,
                    ErrorStrokeThickness,
                    LineStyle.Solid.GetDashArray(),
                    LineJoin.Miter,
                    true);

                var upperLeftErrorPoint = Transform(leftValue, upperValue);
                var upperRightErrorPoint = Transform(rightValue, upperValue);
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
