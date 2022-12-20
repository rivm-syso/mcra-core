using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;

namespace MCRA.Utils.Charting.OxyPlot {

    /// <summary>
    /// Represents a series for scatter plots with the possibility to display error bars.
    /// </summary>
    public class CustomScatterErrorSeries : ScatterSeries<CustomScatterErrorPoint> {

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterErrorSeries" /> class.
        /// </summary>
        public CustomScatterErrorSeries() {
            ErrorBarColor = OxyColors.Black;
            ErrorBarStrokeThickness = 1;
            ErrorBarStopWidth = 4.0;
            MinimumErrorSize = 0;
        }

        /// <summary>
        /// Gets or sets the data field for the X error property.
        /// </summary>
        public string DataFieldErrorX { get; set; }

        /// <summary>
        /// Gets or sets the data field for the Y error property.
        /// </summary>
        public string DataFieldErrorY { get; set; }

        /// <summary>
        /// Gets or sets the color of the error bar.
        /// </summary>
        public OxyColor ErrorBarColor { get; set; }

        /// <summary>
        /// Gets or sets the width of the error bar stop.
        /// </summary>
        public double ErrorBarStopWidth { get; set; }

        /// <summary>
        /// Gets or sets the error bar stroke thickness.
        /// </summary>
        public double ErrorBarStrokeThickness { get; set; }

        /// <summary>
        /// Gets or sets the minimum size (relative to <see cref="ScatterSeries{T}.MarkerSize" />) of the error bars to be shown. 
        /// </summary>
        public double MinimumErrorSize { get; set; }

        /// <summary>
        /// Renders the series on the specified rendering context.
        /// </summary>
        /// <param name="rc">
        /// The rendering context.
        /// </param>
        /// <param name="model">
        /// The owner plot model.
        /// </param>
        public override void Render(IRenderContext rc) {
            var clippingRectangle = GetClippingRect();

            var segments = new List<ScreenPoint>();
            foreach (var point in ActualPointsList) {
                if (point == null) {
                    continue;
                }

                if (point.PlotErrorBars) {
                    var middlePoint = XAxis.Transform(point.X, point.Y, YAxis);
                    if (!double.IsNaN(point.ErrorYLower)) {
                        var bottomErrorPoint = XAxis.Transform(point.X, point.ErrorYLower, YAxis);
                        segments.Add(middlePoint);
                        segments.Add(bottomErrorPoint);
                        if (ErrorBarStopWidth > 0) {
                            segments.Add(new ScreenPoint(bottomErrorPoint.X - ErrorBarStopWidth, bottomErrorPoint.Y));
                            segments.Add(new ScreenPoint(bottomErrorPoint.X + ErrorBarStopWidth, bottomErrorPoint.Y));
                        }
                    }
                    if (!double.IsNaN(point.ErrorYUpper)) {
                        var topErrorPoint = XAxis.Transform(point.X, point.ErrorYUpper, YAxis);
                        segments.Add(middlePoint);
                        segments.Add(topErrorPoint);
                        if (ErrorBarStopWidth > 0) {
                            segments.Add(new ScreenPoint(topErrorPoint.X - ErrorBarStopWidth, topErrorPoint.Y));
                            segments.Add(new ScreenPoint(topErrorPoint.X + ErrorBarStopWidth, topErrorPoint.Y));
                        }
                    }
                    if (!double.IsNaN(point.ErrorXLower)) {
                        var bottomErrorPoint = XAxis.Transform(point.ErrorXLower, point.Y, YAxis);
                        segments.Add(middlePoint);
                        segments.Add(bottomErrorPoint);
                        if (ErrorBarStopWidth > 0) {
                            segments.Add(new ScreenPoint(bottomErrorPoint.X, bottomErrorPoint.Y - ErrorBarStopWidth));
                            segments.Add(new ScreenPoint(bottomErrorPoint.X, bottomErrorPoint.Y + ErrorBarStopWidth));
                        }
                    }
                    if (!double.IsNaN(point.ErrorXUpper)) {
                        var topErrorPoint = XAxis.Transform(point.ErrorXUpper, point.Y, YAxis);
                        segments.Add(middlePoint);
                        segments.Add(topErrorPoint);
                        if (ErrorBarStopWidth > 0) {
                            segments.Add(new ScreenPoint(topErrorPoint.X, topErrorPoint.Y - ErrorBarStopWidth));
                            segments.Add(new ScreenPoint(topErrorPoint.X, topErrorPoint.Y + ErrorBarStopWidth));
                        }
                    }
                }
            }

            rc.DrawClippedLineSegments(clippingRectangle, segments, GetSelectableColor(ErrorBarColor), ErrorBarStrokeThickness, null, LineJoin.Bevel, true);
            base.Render(rc);
        }

        /// <summary>
        /// Implements <see cref="ScatterSeries.UpdateFromDataFields()"/>
        /// </summary>
        protected override void UpdateFromDataFields() {
            throw new NotImplementedException();
        }
    }
}
