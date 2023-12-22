using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public sealed class StackedHistogramSeries<T> : XYAxisSeries {

        /// <summary>
        /// This string must be empty otherwise no legenda is made
        /// </summary>
        private readonly string _addLegenda = " ";

        public StackedHistogramSeries() {
            StrokeThickness = 1;
            Palette = OxyPalettes.BlueWhiteRed31;
            Title = _addLegenda;
            Height = 500;
            Width = 500;
            ShowContributions = false;
        }

        public int Height { get; set; }

        public int Width { get; set; }

        /// <summary>
        /// Determines in which order stacks are rendered
        /// </summary>
        public List<string> LegendaLabels { get; set; }

        public bool ShowContributions { get; set; }

        public OxyPalette Palette { get; set; }

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
        public IList<CategorizedHistogramBin<T>> Items { get; set; }

        public override void Render(IRenderContext rc) {
            if (Items.Count == 0) {
                return;
            }

            VerifyAxes();

            var clippingRect = GetClippingRect();

            SetDefaultValues();
            if (LegendaLabels == null) {
                LegendaLabels = Items.SelectMany(c => c.ContributionFractions.Select(lab => lab.Category.ToString())).Distinct().ToList();
            }

            foreach (var item in Items) {
                var fraction = 0d;
                foreach (var cf in item.ContributionFractions) {
                    var fillColor = Palette.Colors[LegendaLabels.IndexOf(cf.Category.ToString())];
                    StrokeColor = OxyColors.Transparent;
                    if (StrokeThickness > 0 && LineStyle != LineStyle.None) {
                        var leftBottom = Transform(new DataPoint(item.XMinValue, (ShowContributions ? 100 : item.Frequency) * fraction));
                        fraction += cf.Contribution;
                        var rightTop = Transform(new DataPoint(item.XMaxValue, (ShowContributions ? 100 : item.Frequency) * fraction));
                        var rect = new OxyRect(leftBottom, rightTop);
                        rc.DrawRectangle(rect, fillColor, StrokeColor, StrokeThickness, EdgeRenderingMode.Automatic);
                    }
                }
            }
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
