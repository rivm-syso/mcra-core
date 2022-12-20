using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    /// <summary>
    /// Represents a point in a <see cref="ScatterErrorSeries" />.
    /// </summary>
    public class CustomScatterErrorPoint : ScatterPoint {

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterErrorPoint"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="errorXLower">The lower bound of X.</param>
        /// <param name="errorXUpper">The upper bound of X.</param>
        /// <param name="y">The y.</param>
        /// <param name="errorYLower">The lower bound of Y.</param>
        /// <param name="errorYUpper">The upper bound of Y.</param>
        /// <param name="plotErrorBars">Specifies whether the error bars should be rendered.</param>
        public CustomScatterErrorPoint(
            double x,
            double errorXLower,
            double errorXUpper,
            double y,
            double errorYLower,
            double errorYUpper,
            bool plotErrorBars = true
        )
            : base(x, y) {
            ErrorXLower = errorXLower;
            ErrorXUpper = errorXUpper;
            ErrorYLower = errorYLower;
            ErrorYUpper = errorYUpper;
            PlotErrorBars = plotErrorBars;
        }

        /// <summary>
        /// Gets or sets the lower bound of X.
        /// </summary>
        public double ErrorXLower { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of X.
        /// </summary>
        public double ErrorXUpper { get; set; }

        /// <summary>
        /// Gets or sets the lower bound of Y.
        /// </summary>
        public double ErrorYLower { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of Y.
        /// </summary>
        public double ErrorYUpper { get; set; }

        /// <summary>
        /// Plot error bars (or not)
        /// </summary>
        public bool PlotErrorBars { get; set; }
    }
}
