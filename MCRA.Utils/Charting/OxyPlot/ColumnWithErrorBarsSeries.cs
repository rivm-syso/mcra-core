using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class ColumnWithErrorBarsSeries : ErrorBarSeries {

        public ColumnWithErrorBarsSeries() {
        }

        /// <summary>
        /// Gets or sets the color of the error bars.
        /// </summary>
        public OxyColor ErrorStrokeColor { get; set; } = OxyColors.Black;

        /// <summary>
        /// Updates the max/minimum values.
        /// </summary>
        protected override void UpdateMaxMin() {
            base.UpdateAxisMaxMin();
            if (Items != null && Items.Any(r => !double.IsNaN(r.Value))) {
                if (this.IsTransposed()) {
                    MinX = Items.Cast<ColumnWithErrorItem>().Select(r => !double.IsNaN(r.Lower) ? r.Lower : r.Value).Min();
                    MaxX = Items.Cast<ColumnWithErrorItem>().Select(r => !double.IsNaN(r.Upper) ? r.Upper : r.Value).Max();
                } else {
                    MinY = Items.Cast<ColumnWithErrorItem>().Select(r => !double.IsNaN(r.Lower) ? r.Lower : r.Value).Min();
                    MaxY = Items.Cast<ColumnWithErrorItem>().Select(r => !double.IsNaN(r.Upper) ? r.Upper : r.Value).Max();
                }
            }
        }

        protected override void RenderItem(
            IRenderContext rc,
            double barValue,
            double categoryValue,
            double actualBarWidth,
            BarItem item,
            OxyRect rect
        ) {
            var errorItem = item as ColumnWithErrorItem;
            if (errorItem == null) {
                return;
            }

            rc.DrawRectangle(
                rect,
                GetSelectableFillColor(ActualFillColor), 
                StrokeColor,
                StrokeThickness,
                EdgeRenderingMode);

            // Render the error
            var lowerValue = errorItem.Lower;
            var upperValue = errorItem.Upper;
            var left = 0.5 - ErrorWidth / 2;
            var right = 0.5 + ErrorWidth / 2;
            var leftValue = categoryValue + (left * actualBarWidth);
            var middleValue = categoryValue + (0.5 * actualBarWidth);
            var rightValue = categoryValue + (right * actualBarWidth);

            var lowerErrorPoint = Transform(new DataPoint(lowerValue, middleValue));
            var upperErrorPoint = Transform(new DataPoint(upperValue, middleValue));
            rc.DrawLine(
                [lowerErrorPoint, upperErrorPoint],
                ErrorStrokeColor,
                ErrorStrokeThickness,
                EdgeRenderingMode,
                LineStyle.Solid.GetDashArray(),
                LineJoin.Miter);

            if (ErrorWidth > 0 && !double.IsNaN(lowerValue)) {
                var lowerLeftErrorPoint = Transform(new DataPoint(lowerValue, leftValue));
                var lowerRightErrorPoint = Transform(new DataPoint(lowerValue, rightValue));
                rc.DrawLine(
                    [lowerLeftErrorPoint, lowerRightErrorPoint],
                    ErrorStrokeColor,
                    StrokeThickness,
                    EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness),
                    null,
                    LineJoin.Miter
                );
            }

            if (ErrorWidth > 0 && !double.IsNaN(upperValue)) {
                var upperLeftErrorPoint = Transform(new DataPoint(upperValue, leftValue));
                var upperRightErrorPoint = Transform(new DataPoint(upperValue, rightValue));
                rc.DrawLine(
                    [upperLeftErrorPoint, upperRightErrorPoint],
                    ErrorStrokeColor,
                    StrokeThickness,
                    EdgeRenderingMode.GetActual(EdgeRenderingMode.PreferSharpness),
                    null,
                    LineJoin.Miter
                );
            }

            var label = errorItem.Label;
            var labelScreenPoint = new ScreenPoint(upperErrorPoint.X, upperErrorPoint.Y - ActualFontSize);
            rc.DrawText(
                labelScreenPoint,
                label,
                ActualTextColor,
                ActualFont,
                ActualFontSize,
                ActualFontWeight,
                0,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);
        }
    }
}
