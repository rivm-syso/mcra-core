using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    /// <summary>
    /// Draws a gradient square to stress the impact of low, neutral, and high x-values
    /// in a plot.
    /// </summary>
    public class HorizontalHeatMapSeries : XYAxisSeries {

        protected OxyImage _gradientImage;

        /// <summary>
        /// The x-value that is considered as low in the context of the gradient rectangle.
        /// </summary>
        public double XLow { get; set; }

        /// <summary>
        /// The x-value that is considered as high in the context of the gradient rectangle.
        /// </summary>
        public double XHigh { get; set; }

        /// <summary>
        /// The heat map mapping function (should return values in the range [-1; 1].
        /// </summary>
        public Func<double, double, double> HeatMapMappingFunction { get; set; }

        /// <summary>
        /// The lower y limit of the gradient rectangle.
        /// </summary>
        public double YLow { get; set; }

        /// <summary>
        /// The upper y limit of the gradient rectangle.
        /// </summary>
        public double YHigh { get; set; }

        /// <summary>
        /// Grid resolution of the heatmap of X axis.
        /// </summary>
        public int ResolutionX { get; set; }

        /// <summary>
        /// Grid resolution of the heatmap of Y axis.
        /// </summary>
        public int ResolutionY { get; set; }

        /// <summary>
        /// Gets or sets the color axis.
        /// </summary>
        /// <value>The color axis.</value>
        public IColorAxis ColorAxis { get; protected set; }

        /// <summary>
        /// Gets or sets the color axis key.
        /// </summary>
        /// <value>The color axis key.</value>
        public string ColorAxisKey { get; set; }

        public HorizontalHeatMapSeries() {
            ResolutionX = 50;
            ResolutionY = 50;
        }

        /// <summary>
        /// Ensures that the axes of the series is defined.
        /// </summary>
        protected override void EnsureAxes() {
            base.EnsureAxes();
            ColorAxis = PlotModel.GetAxisOrDefault(ColorAxisKey, (Axis)PlotModel.DefaultColorAxis) as IColorAxis;
        }

        /// <summary>
        /// Renders the series on the specified render context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        public override void Render(IRenderContext rc) {
            var s00 = Transform(new DataPoint(XLow, YLow));
            var s11 = Transform(new DataPoint(XHigh, YHigh));
            var rect = new OxyRect(s00, s11);

            if (_gradientImage == null) {
                updateImage();
            }

            var clip = GetClippingRect();
            if (_gradientImage != null) {
                rc.DrawImage(_gradientImage, rect.Left, rect.Top, rect.Width, rect.Height, 1, true);
            }
        }

        /// <summary>
        /// Updates the image with vertical zone's.
        /// </summary>
        protected virtual void updateImage() {
            var buffer = new OxyColor[ResolutionX, ResolutionY];
            for (int i = 0; i < ResolutionX; i++) {
                for (int j = 0; j < ResolutionY; j++) {
                    var p = map(i, j, ResolutionX, ResolutionY);
                    var heat = HeatMapMappingFunction(p.X, p.Y);
                    buffer[i, ResolutionY - j - 1] = heatToColor(heat);
                    //buffer[i, j] = heatToColor(heat);
                }
            }
            _gradientImage = OxyImage.Create(buffer, ImageFormat.Png);
        }

        protected DataPoint map(int i, int j, int pointsX, int pointsY) {
            var screenRectangle = this.GetScreenRectangle();
            var xScreen = screenRectangle.Left + ((double)i / (pointsX - 1)) * (screenRectangle.Right - screenRectangle.Left);
            var yScreen = screenRectangle.Bottom + ((double)j / (pointsY - 1)) * (screenRectangle.Top - screenRectangle.Bottom);
            var x = this.XAxis.InverseTransform(xScreen);
            var y = this.YAxis.InverseTransform(yScreen);
            return new DataPoint(x, y);
        }

        /// <summary>
        /// This one has a more moderate gradient
        /// </summary>
        /// <param name="heat"></param>
        /// <returns></returns>
        protected OxyColor heatToColor(double heat) {
            var min = 100;
            var max = 200;
            if (heat < 0) {
                // Map "red" region
                var reheat = (heat < -1) ? -1 : heat;
                var g = (byte)((1 - Math.Pow(reheat, 2)) * 255);
                var a = (byte)(-reheat * (max - min) + min);
                return OxyColor.FromArgb(a, 255, g, 0);
            } else {
                // Map "green" region
                var reheat = (heat > 1) ? 1 : heat;
                var r = (byte)((1 - Math.Pow(reheat, 2)) * 255);
                var a = (byte)(reheat * (max - min) + min);
                return OxyColor.FromArgb(a, r, 255, 0);
            }
        }

        protected OxyColor heatToColorOriginal(double heat) {
            if (heat < 0) {
                // Map "red" region
                var reheat = (heat < -1) ? -1 : heat;
                var g = (byte)((1 + reheat) * 255);
                return OxyColor.FromArgb(200, 255, g, 0);
            } else {
                // Map "green" region
                var reheat = (heat > 1) ? 1 : heat;
                var r = (byte)((1 - reheat) * 255);
                return OxyColor.FromArgb(200, r, 255, 0);
            }
        }
    }
}
