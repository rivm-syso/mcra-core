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
            this.StrokeThickness = 1;
            this.Palette = OxyPalettes.BlueWhiteRed31;
            this.Title = _addLegenda;
            this.Height = 500;
            this.Width = 500;
            this.ShowContributions = false;
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
        public OxyColor ActualFillColor => this.FillColor.IsUndefined() ? default : this.FillColor;

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
            if (this.Items.Count == 0) {
                return;
            }

            this.VerifyAxes();

            var clippingRect = this.GetClippingRect();

            this.SetDefaultValues();
            if (LegendaLabels == null) {
                LegendaLabels = this.Items.SelectMany(c => c.ContributionFractions.Select(lab => lab.Category.ToString())).Distinct().ToList();
            }

            foreach (var item in this.Items) {
                var fraction = 0d;
                foreach (var cf in item.ContributionFractions) {
                    var fillColor = this.Palette.Colors[this.LegendaLabels.IndexOf(cf.Category.ToString())];
                    StrokeColor = OxyColors.Transparent;
                    if (this.StrokeThickness > 0 && this.LineStyle != LineStyle.None) {
                        var leftBottom = this.Transform(new DataPoint(item.XMinValue, (ShowContributions ? 100 : item.Frequency) * fraction));
                        fraction += cf.Contribution;
                        var rightTop = this.Transform(new DataPoint(item.XMaxValue, (ShowContributions ? 100 : item.Frequency) * fraction));
                        var rect = new OxyRect(leftBottom, rightTop);
                        rc.DrawRectangle(rect, fillColor, this.StrokeColor, this.StrokeThickness, EdgeRenderingMode.Automatic);
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
            foreach (var bar in this.Items) {
                xmin = Math.Min(xmin, bar.XMinValue);
                xmax = Math.Max(xmax, bar.XMaxValue);
                ymax = Math.Max(ymax, bar.Frequency);
            }
            this.MinX = Math.Max(this.XAxis.FilterMinValue, xmin);
            this.MaxX = Math.Min(this.XAxis.FilterMaxValue, xmax);
            this.MaxY = Math.Min(this.YAxis.FilterMaxValue, ymax);
        }
    }
}
