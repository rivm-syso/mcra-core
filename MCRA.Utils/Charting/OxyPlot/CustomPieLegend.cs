using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class CustomPieLegend : LegendBase {

        private OxyRect legendBox;
        /// <summary>
        /// Initializes a new insance of the Legend class.
        /// </summary>
        public CustomPieLegend() {
            this.IsLegendVisible = true;
            this.legendBox = new OxyRect();
            this.Key = null;
            this.GroupNameFont = null;
            this.GroupNameFontWeight = FontWeights.Normal;
            this.GroupNameFontSize = double.NaN;

            this.LegendTitleFont = null;
            this.LegendTitleFontSize = double.NaN;
            this.LegendTitleFontWeight = FontWeights.Bold;
            this.LegendFontSize = 10;
            this.LegendFontWeight = FontWeights.Normal;
            this.LegendSymbolLength = 23;
            this.LegendSymbolWidth = 9;
            this.LegendSymbolMargin = 2;
            this.LegendPadding = 8;
            this.LegendColumnSpacing = 8;
            this.LegendItemSpacing = 24;
            this.LegendLineSpacing = 0;
            this.LegendMargin = 5;

            this.LegendBackground = OxyColors.White;
            this.LegendBorder = OxyColors.White;
            this.LegendBorderThickness = 1;

            this.LegendTextColor = OxyColors.Black;
            this.LegendTitleColor = OxyColors.Automatic;

            this.LegendMaxWidth = double.NaN;
            this.LegendMaxHeight = double.NaN;
            this.LegendPlacement = LegendPlacement.Inside;
            this.LegendPosition = LegendPosition.RightMiddle;
            this.LegendOrientation = LegendOrientation.Vertical;
            this.LegendItemOrder = LegendItemOrder.Normal;
            this.LegendItemAlignment = HorizontalAlignment.Left;
            this.LegendSymbolPlacement = LegendSymbolPlacement.Left;

            this.ShowInvisibleSeries = true;

            this.SeriesInvisibleTextColor = OxyColor.FromAColor(64, this.LegendTextColor);

            this.SeriesPosMap = [];

            this.Selectable = true;
            this.SelectionMode = SelectionMode.Single;
        }

        public double LegendSymbolWidth { get; set; }

        /// <summary>
        /// Override for legend hit test.
        /// </summary>
        /// <param name="args">Arguments passe to the hit test</param>
        /// <returns>The hit test results.</returns>
        protected override HitTestResult LegendHitTest(HitTestArguments args) {
            ScreenPoint point = args.Point;
            if (this.IsPointInLegend(point)) {
                if (this.SeriesPosMap != null && this.SeriesPosMap.Count > 0) {
                    foreach (KeyValuePair<Series, OxyRect> kvp in this.SeriesPosMap) {
                        if (kvp.Value.Contains(point)) {
                            if (this.ShowInvisibleSeries) {
                                kvp.Key.IsVisible = !kvp.Key.IsVisible;
                                this.PlotModel.InvalidatePlot(false);
                                break;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the group name font.
        /// </summary>
        public string GroupNameFont {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the group name font size.
        /// </summary>
        public double GroupNameFontSize {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the group name font weight.
        /// </summary>
        public double GroupNameFontWeight {
            get;
            set;
        }

        private Dictionary<Series, OxyRect> SeriesPosMap { get; set; }

        /// <summary>
        /// Gets or sets the textcolor of invisible series.
        /// </summary>
        public OxyColor SeriesInvisibleTextColor { get; set; }

        /// <summary>
        /// Checks if a screen point is within the legend boundaries.
        /// </summary>
        /// <param name="point">A screen point.</param>
        /// <returns>A value indicating whether the point is inside legend boundaries or not.</returns>
        public bool IsPointInLegend(ScreenPoint point) {
            return this.legendBox.Contains(point);
        }

        /// <summary>
        /// Makes the LegendOrientation property safe.
        /// </summary>
        /// <remarks>If Legend is positioned left or right, force it to vertical orientation</remarks>
        public override void EnsureLegendProperties() {
            switch (this.LegendPosition) {
                case LegendPosition.LeftTop:
                case LegendPosition.LeftMiddle:
                case LegendPosition.LeftBottom:
                case LegendPosition.RightTop:
                case LegendPosition.RightMiddle:
                case LegendPosition.RightBottom:
                    if (this.LegendOrientation == LegendOrientation.Horizontal) {
                        this.LegendOrientation = LegendOrientation.Vertical;
                    }

                    break;
            }
        }

        /// <summary>
        /// Renders or measures the legends.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public override void RenderLegends(IRenderContext rc) {
            this.RenderOrMeasureLegends(rc, this.LegendArea);
        }

        /// <summary>
        /// Measures the legend area and gets the legend size.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        /// <param name="availableLegendArea">The area available to legend.</param>
        public override OxySize GetLegendSize(IRenderContext rc, OxySize availableLegendArea) {
            var availableLegendWidth = availableLegendArea.Width;
            var availableLegendHeight = availableLegendArea.Height;

            // Calculate the size of the legend box
            var legendSize = this.MeasureLegends(rc, new OxySize(Math.Max(0, availableLegendWidth), Math.Max(0, availableLegendHeight)));

            // Ensure legend size is valid
            legendSize = new OxySize(Math.Max(0, legendSize.Width), Math.Max(0, legendSize.Height));

            return legendSize;
        }

        /// <summary>
        /// Gets the rectangle of the legend box.
        /// </summary>
        /// <param name="legendSize">Size of the legend box.</param>
        /// <returns>A rectangle.</returns>
        public override OxyRect GetLegendRectangle(OxySize legendSize) {
            double top = 0;
            double left = 0;
            if (this.LegendPlacement == LegendPlacement.Outside) {
                switch (this.LegendPosition) {
                    case LegendPosition.LeftTop:
                    case LegendPosition.LeftMiddle:
                    case LegendPosition.LeftBottom:
                        left = this.PlotModel.PlotAndAxisArea.Left - legendSize.Width - this.LegendMargin;
                        break;
                    case LegendPosition.RightTop:
                    case LegendPosition.RightMiddle:
                    case LegendPosition.RightBottom:
                        left = this.PlotModel.PlotAndAxisArea.Right + this.LegendMargin;
                        break;
                    case LegendPosition.TopLeft:
                    case LegendPosition.TopCenter:
                    case LegendPosition.TopRight:
                        top = this.PlotModel.PlotAndAxisArea.Top - legendSize.Height - this.LegendMargin;
                        break;
                    case LegendPosition.BottomLeft:
                    case LegendPosition.BottomCenter:
                    case LegendPosition.BottomRight:
                        top = this.PlotModel.PlotAndAxisArea.Bottom + this.LegendMargin;
                        break;
                }

                var bounds = this.AllowUseFullExtent
                    ? this.PlotModel.PlotAndAxisArea
                    : this.PlotModel.PlotArea;

                switch (this.LegendPosition) {
                    case LegendPosition.TopLeft:
                    case LegendPosition.BottomLeft:
                        left = bounds.Left;
                        break;
                    case LegendPosition.TopRight:
                    case LegendPosition.BottomRight:
                        left = bounds.Right - legendSize.Width;
                        break;
                    case LegendPosition.LeftTop:
                    case LegendPosition.RightTop:
                        top = bounds.Top;
                        break;
                    case LegendPosition.LeftBottom:
                    case LegendPosition.RightBottom:
                        top = bounds.Bottom - legendSize.Height;
                        break;
                    case LegendPosition.LeftMiddle:
                    case LegendPosition.RightMiddle:
                        top = (bounds.Top + bounds.Bottom - legendSize.Height) * 0.5;
                        break;
                    case LegendPosition.TopCenter:
                    case LegendPosition.BottomCenter:
                        left = (bounds.Left + bounds.Right - legendSize.Width) * 0.5;
                        break;
                }
            } else {
                switch (this.LegendPosition) {
                    case LegendPosition.LeftTop:
                    case LegendPosition.LeftMiddle:
                    case LegendPosition.LeftBottom:
                        left = this.PlotModel.PlotArea.Left + this.LegendMargin;
                        break;
                    case LegendPosition.RightTop:
                    case LegendPosition.RightMiddle:
                    case LegendPosition.RightBottom: {
                            // Position the legend 10 units to the right of the pie chart
                            var customPieSeries = GetCustomPieSeries();
                            left = customPieSeries.MidPoint.X + customPieSeries.OuterRadius + this.LegendMargin;
                        }
                        break;
                    case LegendPosition.TopLeft:
                    case LegendPosition.TopCenter:
                    case LegendPosition.TopRight:
                        top = this.PlotModel.PlotArea.Top + this.LegendMargin;
                        break;
                    case LegendPosition.BottomLeft:
                    case LegendPosition.BottomCenter:
                    case LegendPosition.BottomRight:
                        top = this.PlotModel.PlotArea.Bottom - legendSize.Height - this.LegendMargin;
                        break;
                }

                switch (this.LegendPosition) {
                    case LegendPosition.TopLeft:
                    case LegendPosition.BottomLeft:
                        left = this.PlotModel.PlotArea.Left + this.LegendMargin;
                        break;
                    case LegendPosition.TopRight:
                    case LegendPosition.BottomRight:
                        left = this.PlotModel.PlotArea.Right - legendSize.Width - this.LegendMargin;
                        break;
                    case LegendPosition.LeftTop:
                    case LegendPosition.RightTop:
                        top = this.PlotModel.PlotArea.Top + this.LegendMargin;
                        break;
                    case LegendPosition.LeftBottom:
                    case LegendPosition.RightBottom:
                        top = this.PlotModel.PlotArea.Bottom - legendSize.Height - this.LegendMargin;
                        break;

                    case LegendPosition.LeftMiddle:
                    case LegendPosition.RightMiddle:
                        top = (this.PlotModel.PlotArea.Top + this.PlotModel.PlotArea.Bottom - legendSize.Height) * 0.5;
                        break;
                    case LegendPosition.TopCenter:
                    case LegendPosition.BottomCenter:
                        left = (this.PlotModel.PlotArea.Left + this.PlotModel.PlotArea.Right - legendSize.Width) * 0.5;
                        break;
                }
            }

            return new OxyRect(left, top, legendSize.Width, legendSize.Height);
        }

        /// <summary>
        /// Measures the legends.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="availableSize">The available size for the legend box.</param>
        /// <returns>The size of the legend box.</returns>
        private OxySize MeasureLegends(IRenderContext rc, OxySize availableSize) {
            return this.RenderOrMeasureLegends(rc, new OxyRect(0, 0, availableSize.Width, availableSize.Height), true);
        }

        /// <summary>
        /// Renders or measures the legends.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="rect">Provides the available size if measuring, otherwise it provides the position and size of the legend.</param>
        /// <param name="measureOnly">Specify if the size of the legend box should be measured only (not rendered).</param>
        /// <returns>The size of the legend box.</returns>
        private OxySize RenderOrMeasureLegends(IRenderContext rc, OxyRect rect, bool measureOnly = false) {
            var customPieSeries = GetCustomPieSeries();

            var actualLegendFontSize = double.IsNaN(this.LegendFontSize) ? this.PlotModel.DefaultFontSize : this.LegendFontSize;
            var actualLegendFont = this.LegendFont ?? this.PlotModel.DefaultFont;

            var counter = 0;
            double availableWidth = rect.Width;
            double availableHeight = rect.Height;
            double maxItemWidth = 0;
            double totalHeight = 0;
            foreach (var slice in customPieSeries.Slices) {
                var textSize = rc.MeasureMathText(slice.Label, this.LegendFont ?? this.PlotModel.DefaultFont, actualLegendFontSize, this.LegendFontWeight);
                var width = this.LegendSymbolLength + this.LegendSymbolMargin + textSize.Width;
                var height = this.LegendLineSpacing + textSize.Height;

                maxItemWidth = Math.Max(maxItemWidth, width);
                totalHeight += height;

                if (!measureOnly) {
                    // Draw box
                    var xBox = rect.Left + this.LegendMargin;
                    var yBox = rect.Top + this.LegendMargin + (totalHeight - height);
                    rc.DrawRectangle(new OxyRect(xBox, yBox, this.LegendSymbolLength, this.LegendSymbolWidth), customPieSeries.Pallete.Colors[counter++], OxyColors.Undefined, 1, EdgeRenderingMode.Automatic);

                    // Draw text
                    var xTxt = rect.Left + this.LegendMargin + this.LegendSymbolLength + this.LegendSymbolMargin;
                    var yTxt = yBox - 0.1 * textSize.Height;
                    var maxTxtWidth = Math.Max(textSize.Width, availableWidth - this.LegendSymbolLength + this.LegendSymbolMargin - 2 * this.LegendMargin);
                    var charsPerWidth = slice.Label.Length / textSize.Width;
                    var maxChars = (int)(maxTxtWidth * charsPerWidth);
                    var nrOfChars = Math.Max(slice.Label.Length, maxChars);
                    var label = slice.Label.LimitTo(nrOfChars);
                    rc.DrawText(new ScreenPoint(xTxt, yTxt), label, this.LegendTextColor,
                                actualLegendFont, actualLegendFontSize, this.LegendFontWeight, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
                }
            }

            maxItemWidth += 2 * this.LegendMargin;
            totalHeight += 2 * this.LegendMargin;

            var size = new OxySize(maxItemWidth, totalHeight);
            if (size.Width > 0) {
                size = new OxySize(size.Width + this.LegendPadding, size.Height);
            }

            if (size.Height > 0) {
                size = new OxySize(size.Width, size.Height + this.LegendPadding);
            }

            if (size.Width > availableWidth) {
                size = new OxySize(availableWidth, size.Height);
            }

            if (size.Height > availableHeight) {
                size = new OxySize(size.Width, availableHeight);
            }

            if (!double.IsNaN(this.LegendMaxWidth) && size.Width > this.LegendMaxWidth) {
                size = new OxySize(this.LegendMaxWidth, size.Height);
            }

            if (!double.IsNaN(this.LegendMaxHeight) && size.Height > this.LegendMaxHeight) {
                size = new OxySize(size.Width, this.LegendMaxHeight);
            }

            return size;
        }
        private CustomPieSeries GetCustomPieSeries() {
            if (this.PlotModel.Series.Count != 1) {
                throw new InvalidOperationException();
            }
            var customPieSeries = this.PlotModel.Series.First() as CustomPieSeries;
            return customPieSeries;
        }
    }
}
