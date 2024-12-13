using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    /// <summary>
    /// Represents a series for box plots.
    /// </summary>
    public class MultipleWhiskerHorizontalBoxPlotSeries : XYAxisSeries {
        /// <summary>
        /// The default tracker format string
        /// </summary>
        public new const string DefaultTrackerFormatString = "{0}\n{1}: {2}\nUpper Whisker: {3:N2}\nThird Quartil: {4:N2}\nMedian: {5:N2}\nFirst Quartil: {6:N2}\nLower Whisker: {7:N2}\nMean: {8:N2}";

        /// <summary>
        /// The items from the items source.
        /// </summary>
        private List<MultipleWhiskerBoxPlotItem> itemsSourceItems;

        /// <summary>
        /// Specifies if the ownsItemsSourceItems list can be modified.
        /// </summary>
        private bool ownsItemsSourceItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxPlotSeries" /> class.
        /// </summary>
        public MultipleWhiskerHorizontalBoxPlotSeries() {
            Items = [];
            TrackerFormatString = DefaultTrackerFormatString;
            OutlierTrackerFormatString = "{0}\n{1}: {2}\nY: {3:0.00}";
            Title = null;
            Fill = OxyColors.Automatic;
            Stroke = OxyColors.Black;
            BoxWidth = 0.3;
            StrokeThickness = 1;
            MedianThickness = 2;
            MeanThickness = 2;
            OutlierSize = 2;
            OutlierType = MarkerType.Circle;
            MedianPointSize = 2;
            MeanPointSize = 2;
            WhiskerWidth = 0.5;
            LineStyle = LineStyle.Solid;
            ShowMedianAsDot = false;
            ShowMeanAsDot = false;
            ShowBox = true;
        }

        /// <summary>
        /// Gets or sets the width of the boxes (specified in x-axis units).
        /// </summary>
        /// <value>The width of the boxes.</value>
        public double BoxWidth { get; set; }

        /// <summary>
        /// Gets or sets the fill color. If <c>null</c>, this color will be automatically set.
        /// </summary>
        /// <value>The fill color.</value>
        public OxyColor Fill { get; set; }

        /// <summary>
        /// Gets or sets the box plot items.
        /// </summary>
        /// <value>The items.</value>
        public List<MultipleWhiskerBoxPlotItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        /// <value>The line style.</value>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the size of the median point.
        /// </summary>
        /// <remarks>This property is only used when ShowMedianAsDot = true.</remarks>
        public double MedianPointSize { get; set; }

        /// <summary>
        /// Gets or sets the median thickness, relative to the StrokeThickness.
        /// </summary>
        /// <value>The median thickness.</value>
        public double MedianThickness { get; set; }

        /// <summary>
        /// Gets or sets the size of the mean point.
        /// </summary>
        /// <remarks>This property is only used when ShowMeanAsDot = true.</remarks>
        public double MeanPointSize { get; set; }

        /// <summary>
        /// Gets or sets the mean thickness, relative to the StrokeThickness.
        /// </summary>
        /// <value>The mean thickness.</value>
        public double MeanThickness { get; set; }

        /// <summary>
        /// Gets or sets the diameter of the outlier circles (specified in points).
        /// </summary>
        /// <value>The size of the outlier.</value>
        public double OutlierSize { get; set; }

        /// <summary>
        /// Gets or sets the tracker format string for the outliers.
        /// </summary>
        /// <value>The tracker format string for the outliers.</value>
        /// <remarks>Use {0} for series title, {1} for x- and {2} for y-value.</remarks>
        public string OutlierTrackerFormatString { get; set; }

        /// <summary>
        /// Gets or sets the type of the outliers.
        /// </summary>
        /// <value>The type of the outliers.</value>
        /// <remarks>MarkerType.Custom is currently not supported.</remarks>
        public MarkerType OutlierType { get; set; }

        /// <summary>
        /// Gets or sets the a custom polygon outline for the outlier markers. Set <see cref="OutlierType" /> to <see cref="OxyPlot.MarkerType.Custom" /> to use this property.
        /// </summary>
        /// <value>A polyline. The default is <c>null</c>.</value>
        public ScreenPoint[] OutlierOutline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the boxes.
        /// </summary>
        public bool ShowBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the median as a dot.
        /// </summary>
        public bool ShowMedianAsDot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the mean as a dot.
        /// </summary>
        public bool ShowMeanAsDot { get; set; }

        /// <summary>
        /// Gets or sets the stroke color.
        /// </summary>
        /// <value>The stroke color.</value>
        public OxyColor Stroke { get; set; }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        /// <value>The stroke thickness.</value>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Gets or sets the width of the whiskers (relative to the BoxWidth).
        /// </summary>
        /// <value>The width of the whiskers.</value>
        public double WhiskerWidth { get; set; }

        /// <summary>
        /// Gets the list of items that should be rendered.
        /// </summary>
        protected IList<MultipleWhiskerBoxPlotItem> ActualItems {
            get {
                return ItemsSource != null ? itemsSourceItems : Items;
            }
        }

        /// <summary>
        /// Determines whether the specified item contains a valid point.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="xaxis">The x axis.</param>
        /// <param name="yaxis">The y axis.</param>
        /// <returns><c>true</c> if the point is valid; otherwise, <c>false</c> .</returns>
        public virtual bool IsValidPoint(MultipleWhiskerBoxPlotItem item, Axis xaxis, Axis yaxis) {
            return !double.IsNaN(item.X) && !double.IsInfinity(item.X) && !item.Values.Any(double.IsNaN)
                   && !item.Values.Any(double.IsInfinity) && (yaxis != null && yaxis.IsValidValue(item.X))
                   && (xaxis != null && item.Values.All(xaxis.IsValidValue));
        }

        /// <summary>
        /// Renders the series on the specified render context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        public override void Render(IRenderContext rc) {
            if (ActualItems.Count == 0) {
                return;
            }

            var clippingRect = GetClippingRect();

            var outlierScreenPoints = new List<ScreenPoint>();
            var halfBoxWidth = BoxWidth * 0.5;
            var halfWhiskerWidth = halfBoxWidth * WhiskerWidth;
            var strokeColor = GetSelectableColor(Stroke);
            var fillColor = GetSelectableFillColor(Fill);

            var dashArray = LineStyle.GetDashArray();

            foreach (var item in ActualItems) {
                // Add the outlier points
                outlierScreenPoints.AddRange(item.Outliers.Select(outlier => Transform(new DataPoint(outlier, item.X))));
                var upperWhisker = item.UpperWhisker > item.MaxWhisker ? item.UpperWhisker : item.MaxWhisker;
                var lowerWhisker = item.LowerWhisker < item.MinWhisker ? item.LowerWhisker : item.MinWhisker;
                var topWhiskerTop = Transform(new DataPoint(upperWhisker, item.X));
                var topWhiskerBottom = Transform(new DataPoint(item.BoxTop, item.X));
                var bottomWhiskerTop = Transform(new DataPoint(item.BoxBottom, item.X));
                var bottomWhiskerBottom = Transform(new DataPoint(lowerWhisker, item.X));
                rc.DrawLine(
                    [topWhiskerTop, topWhiskerBottom],
                    strokeColor,
                    StrokeThickness,
                    EdgeRenderingMode.Automatic,
                    dashArray,
                    LineJoin.Miter);
                rc.DrawLine(
                    [bottomWhiskerTop, bottomWhiskerBottom],
                    strokeColor,
                    StrokeThickness,
                    EdgeRenderingMode.Automatic,
                    dashArray,
                    LineJoin.Miter);

                // Draw the whiskers
                if (WhiskerWidth > 0) {
                    var topWhiskerLine1 = Transform(new DataPoint(item.UpperWhisker, item.X - halfWhiskerWidth));
                    var topWhiskerLine2 = Transform(new DataPoint(item.UpperWhisker, item.X + halfWhiskerWidth));
                    var bottomWhiskerLine1 = Transform(new DataPoint(item.LowerWhisker, item.X - halfWhiskerWidth));
                    var bottomWhiskerLine2 = Transform(new DataPoint(item.LowerWhisker, item.X + halfWhiskerWidth));
                    var top2WhiskerLine1 = Transform(new DataPoint(item.MaxWhisker, item.X - halfWhiskerWidth));
                    var top2WhiskerLine2 = Transform(new DataPoint(item.MaxWhisker, item.X + halfWhiskerWidth));
                    var bottom2WhiskerLine1 = Transform(new DataPoint(item.MinWhisker, item.X - halfWhiskerWidth));
                    var bottom2WhiskerLine2 = Transform(new DataPoint(item.MinWhisker, item.X + halfWhiskerWidth));
                    rc.DrawLine(
                        [topWhiskerLine1, topWhiskerLine2],
                        strokeColor,
                        StrokeThickness,
                        EdgeRenderingMode.Automatic,
                        null,
                        LineJoin.Miter);
                    rc.DrawLine(
                        [bottomWhiskerLine1, bottomWhiskerLine2],
                        strokeColor,
                        StrokeThickness,
                        EdgeRenderingMode.Automatic,
                        null,
                        LineJoin.Miter);
                    rc.DrawLine(
                       [top2WhiskerLine1, top2WhiskerLine2],
                       strokeColor,
                       StrokeThickness,
                       EdgeRenderingMode.Automatic,
                       null,
                       LineJoin.Miter);
                    rc.DrawLine(
                        [bottom2WhiskerLine1, bottom2WhiskerLine2],
                        strokeColor,
                        StrokeThickness,
                        EdgeRenderingMode.Automatic,
                        null,
                        LineJoin.Miter);
                }

                if (ShowBox) {
                    // Draw the box
                    var rect = GetBoxRect(item);
                    rc.DrawRectangle(rect, fillColor, strokeColor, StrokeThickness, EdgeRenderingMode.Automatic);
                }

                if (!ShowMedianAsDot) {
                    // Draw the median line
                    var medianLeft = Transform(new DataPoint(item.Median, item.X - halfBoxWidth));
                    var medianRight = Transform(new DataPoint(item.Median, item.X + halfBoxWidth));
                    rc.DrawLine(
                        [medianLeft, medianRight],
                        strokeColor,
                        StrokeThickness * MedianThickness,
                        EdgeRenderingMode.Automatic,
                        null,
                        LineJoin.Miter);
                } else {
                    var mc = Transform(new DataPoint(item.Median, item.X));
                    if (clippingRect.Contains(mc)) {
                        var ellipseRect = new OxyRect(
                            mc.X - MedianPointSize,
                            mc.Y - MedianPointSize,
                            MedianPointSize * 2,
                            MedianPointSize * 2);
                        rc.DrawEllipse(ellipseRect, fillColor, OxyColors.Undefined, 0, EdgeRenderingMode.Automatic);
                    }
                }

                if (!ShowMeanAsDot && !double.IsNaN(item.Median)) {
                    // Draw the median line
                    var meanLeft = Transform(new DataPoint(item.Median, item.X - halfBoxWidth));
                    var meanRight = Transform(new DataPoint(item.Median, item.X + halfBoxWidth));
                    rc.DrawLine(
                        [meanLeft, meanRight],
                        strokeColor,
                        StrokeThickness * MeanThickness,
                        EdgeRenderingMode.Automatic,
                        LineStyle.Dash.GetDashArray(),
                        LineJoin.Miter);
                } else if (!double.IsNaN(item.Median)) {
                    var mc = Transform(new DataPoint(item.Median, item.X));
                    if (clippingRect.Contains(mc)) {
                        var ellipseRect = new OxyRect(
                            mc.X - MeanPointSize,
                            mc.Y - MeanPointSize,
                            MeanPointSize * 2,
                            MeanPointSize * 2);
                        rc.DrawEllipse(ellipseRect, fillColor, OxyColors.Undefined, 0, EdgeRenderingMode.Automatic);
                    }
                }

                // Draw the LOR whiskers
                if (item.LowerBound > 0) {
                    var lorLine1 = Transform(new DataPoint(item.LowerBound, item.X - halfWhiskerWidth));
                    var lorLine2 = Transform(new DataPoint(item.LowerBound, item.X + halfWhiskerWidth));
                    rc.DrawLine(
                        [lorLine1, lorLine2],
                        OxyColors.Red,
                        StrokeThickness,
                        EdgeRenderingMode.Automatic,
                        null,
                        LineJoin.Miter);
                }
            }

            if (OutlierType != MarkerType.None) {
                // Draw the outlier(s)
                var markerSizes = outlierScreenPoints.Select(o => OutlierSize).ToList();
                rc.DrawMarkers(
                    outlierScreenPoints,
                    OutlierType,
                    OutlierOutline,
                    markerSizes,
                    fillColor,
                    strokeColor,
                    StrokeThickness,
                    EdgeRenderingMode.Automatic);
            }
        }

        /// <summary>
        /// Updates the data.
        /// </summary>
        protected override void UpdateData() {
            if (ItemsSource == null) {
                return;
            }

            var sourceAsListOfT = ItemsSource as IEnumerable<MultipleWhiskerBoxPlotItem>;
            if (sourceAsListOfT != null) {
                itemsSourceItems = sourceAsListOfT.ToList();
                ownsItemsSourceItems = false;
                return;
            }

            ClearItemsSourceItems();

            itemsSourceItems.AddRange(ItemsSource.OfType<MultipleWhiskerBoxPlotItem>());
        }

        /// <summary>
        /// Updates the maximum and minimum values of the series.
        /// </summary>
        protected override void UpdateMaxMin() {
            base.UpdateMaxMin();
            InternalUpdateMaxMin(ActualItems);
        }

        /// <summary>
        /// Updates the max and min of the series.
        /// </summary>
        /// <param name="items">The items.</param>
        protected void InternalUpdateMaxMin(IList<MultipleWhiskerBoxPlotItem> items) {
            if (items == null || items.Count == 0) {
                return;
            }

            double minx = MinX;
            double miny = MinY;
            double maxx = MaxX;
            double maxy = MaxY;

            foreach (var pt in items) {
                if (!IsValidPoint(pt, XAxis, YAxis)) {
                    continue;
                }

                var y = pt.X;
                if (y < miny || double.IsNaN(miny)) {
                    miny = y;
                }

                if (y > maxy || double.IsNaN(maxy)) {
                    maxy = y;
                }

                foreach (var x in pt.Values) {
                    if (x < minx || double.IsNaN(minx)) {
                        minx = x;
                    }

                    if (x > maxx || double.IsNaN(maxx)) {
                        maxx = x;
                    }
                }
            }

            MinX = minx;
            MinY = miny;
            MaxX = maxx;
            MaxY = maxy;
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="i">The index of the item.</param>
        /// <returns>The item of the index.</returns>
        protected override object GetItem(int i) {
            if (ItemsSource != null || ActualItems == null || ActualItems.Count == 0) {
                return base.GetItem(i);
            }

            return ActualItems[i];
        }

        /// <summary>
        /// Gets the screen rectangle for the box.
        /// </summary>
        /// <param name="item">The box item.</param>
        /// <returns>A rectangle.</returns>
        private OxyRect GetBoxRect(MultipleWhiskerBoxPlotItem item) {
            var halfBoxWidth = BoxWidth * 0.5;
            var boxTop = Transform(new DataPoint(item.BoxTop, item.X - halfBoxWidth));
            var boxBottom = Transform(new DataPoint(item.BoxBottom, item.X + halfBoxWidth));
            var rect = new OxyRect(boxBottom.X, boxBottom.Y, boxTop.X - boxBottom.X, boxTop.Y - boxBottom.Y);
            return rect;
        }

        /// <summary>
        /// Clears or creates the <see cref="itemsSourceItems"/> list.
        /// </summary>
        private void ClearItemsSourceItems() {
            if (!ownsItemsSourceItems || itemsSourceItems == null) {
                itemsSourceItems = [];
            } else {
                itemsSourceItems.Clear();
            }
            ownsItemsSourceItems = true;
        }
    }
}
