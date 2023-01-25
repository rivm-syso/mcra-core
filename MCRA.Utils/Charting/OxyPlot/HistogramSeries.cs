using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public sealed class HistogramSeries : XYAxisSeries {

        public HistogramSeries() {
            StrokeColor = OxyColors.Black;
            StrokeThickness = 1;
            FillColor = OxyColors.DarkGreen;
        }

        /// <summary>
        /// Gets or sets the color of the interior of the bars.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public OxyColor FillColor { get; set; }

        /// <summary>
        /// Gets the actual fill color.
        /// </summary>
        /// <value>The actual color.</value>
        public OxyColor ActualFillColor => FillColor.IsUndefined() ? default : FillColor;

        /// <summary>
        /// Gets or sets the color of the border around the bars.
        /// </summary>
        /// <value>
        /// The color of the stroke.
        /// </value>
        public OxyColor StrokeColor { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the curve.
        /// </summary>
        /// <value>The stroke thickness.</value>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        /// <value>The line style.</value>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// The histogram bins that are to be drawn.
        /// </summary>
        public IList<HistogramBin> Items { get; set; }

        /// <summary>
        /// Renders the series.
        /// </summary>
        public override void Render(IRenderContext rc) {
            if (Items.Count == 0) {
                return;
            }
            VerifyAxes();
            var clippingRect = GetClippingRect();
            var fillColor = GetSelectableFillColor(FillColor);
            SetDefaultValues();
            foreach (var v in Items) {
                if (StrokeThickness > 0 && LineStyle != LineStyle.None) {
                    var leftBottom = Transform(new DataPoint(v.XMinValue, 0));
                    var rightTop = Transform(new DataPoint(v.XMaxValue, v.Frequency));
                    var rect = new OxyRect(leftBottom, rightTop);
                    rc.DrawRectangle(rect, fillColor, StrokeColor, StrokeThickness, EdgeRenderingMode.Automatic);
                }
            }
        }

        /// <summary>
        /// Renders the legend symbol for the line series on the
        /// specified rendering context.
        /// </summary>
        /// <param name="rc">
        /// The rendering context.
        /// </param>
        /// <param name="legendBox">
        /// The bounding rectangle of the legend box.
        /// </param>
        public override void RenderLegend(IRenderContext rc, OxyRect legendBox) {
            double xmid = (legendBox.Left + legendBox.Right) / 2;
            double ymid = (legendBox.Top + legendBox.Bottom) / 2;
            var pts = new[] {
                new ScreenPoint(legendBox.Left, ymid),
                new ScreenPoint(legendBox.Right, ymid)
            };
            rc.DrawLine(
                pts,
                GetSelectableColor(ActualFillColor),
                StrokeThickness,
                EdgeRenderingMode.Automatic,
                LineStyle.GetDashArray());
            var midpt = new ScreenPoint(xmid, ymid);
            rc.DrawMarker(
                midpt,
                MarkerType.None,
                null,
                0,
                FillColor,
                StrokeColor,
                2,
                EdgeRenderingMode.Automatic
            );
        }

        /// <summary>
        /// Updates the maximum and minimum values of the series.
        /// </summary>
        protected override void UpdateMaxMin() {
            base.UpdateMaxMin();
            var xmin = double.MaxValue;
            var xmax = double.MinValue;
            var ymax = double.MinValue;
            foreach (var bar in Items) {
                xmin = Math.Min(xmin, bar.XMinValue);
                xmax = Math.Max(xmax, bar.XMaxValue);
                ymax = Math.Max(ymax, bar.Frequency);
            }
            MinX = Math.Max(XAxis.FilterMinValue, xmin);
            MaxX = Math.Min(XAxis.FilterMaxValue, xmax);
            MaxY = Math.Min(YAxis.FilterMaxValue, ymax);
        }
    }
}
