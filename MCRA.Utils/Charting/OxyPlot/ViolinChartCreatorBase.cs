using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {

    public abstract class ViolinChartCreatorBase : OxyPlotBoxPlotCreator {

        /// <summary>
        /// Scaling constant for envelope and percentiles.
        /// </summary>
        private const double _scale = 0.4;

        /// <summary>
        /// Create logarithmic default axis for horizontal (default) violin plots
        /// </summary>
        /// <returns></returns>
        public LogarithmicAxis CreateLogarithmicAxis(bool horizontal) {
            return new LogarithmicAxis() {
                Position = horizontal ? AxisPosition.Bottom : AxisPosition.Left,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
                MajorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 2
            };
        }

        /// <summary>
        /// Converts the target uncertain datapoint to a boxplot datapoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="axisLabelExtractor">Expression to build the x-axis label for the datapoint</param>
        /// <param name="wiskerType">Calculationmethod for the wiskers</param>
        /// <param name="showOutliers">If true, outliers are shown as points outside the wiskers. If false, outliers are hidden.</param>
        /// <returns></returns>
        protected BoxPlotDataPoint asBoxPlotDataPoint(IEnumerable<double> source, WiskerType wiskerType = WiskerType.ExtremePercentiles, double lowerBound = 2.5, double upperBound = 97.5, bool showOutliers = true, double interQuartileRangeFactor = 1.5) {
            var lowerBox = source.Percentile(25);
            var upperBox = source.Percentile(75);
            if (upperBound < 75) {
                upperBox = source.Percentile(upperBound);
            }
            if (lowerBound > 25) {
                lowerBox = source.Percentile(lowerBound);
            }
            double lowerWisker = 0d;
            double upperWisker = 0d;

            switch (wiskerType) {
                case WiskerType.ExtremePercentiles:
                    lowerWisker = source.Percentile(lowerBound);
                    upperWisker = source.Percentile(upperBound);
                    break;
                case WiskerType.BasedOnInterQuartileRange:
                    var wiskerSize = interQuartileRangeFactor * (upperBox - lowerBox);
                    lowerWisker = lowerBox - wiskerSize;
                    upperWisker = upperBox + wiskerSize;
                    var valuesWithinRange = source.Where(p => p >= lowerWisker && p <= upperWisker);
                    lowerWisker = valuesWithinRange.Min();
                    upperWisker = valuesWithinRange.Max();
                    break;
            }

            var boxPlotdataPoint = new BoxPlotDataPoint() {
                LowerWisker = lowerWisker,
                UpperWisker = upperWisker,
                LowerBox = lowerBox,
                UpperBox = upperBox,
                Median = source.Percentile(50),
                Outliers = showOutliers ? source.Where(v => v > upperWisker || v < lowerWisker).ToList() : [],
            };
            return boxPlotdataPoint;
        }

        protected (Dictionary<string, List<double>>, Dictionary<string, List<double>>, double, double) ComputeKernel(IDictionary<string, (List<double> x, bool skip)> data) {
            var maximumY = double.NegativeInfinity;
            var numberOfValuesRef = double.MinValue;
            var yKernel = new Dictionary<string, List<double>>();
            var xKernel = new Dictionary<string, List<double>>();
            using (var R = new RDotNetEngine()) {
                foreach (var item in data) {
                    var x = new List<double>();
                    var y = new List<double>();
                    if (!item.Value.x.All(c => c == 0)) {
                        try {
                            if (item.Value.skip) {
                                xKernel[item.Key] = null;
                                yKernel[item.Key] = null;
                            } else {
                                R.SetSymbol("data", item.Value.x.Select(c => Math.Log(c)).ToList());
                                R.EvaluateNoReturn("kernel <- density(data, kernel = 'gaussian', adjust = 1)");
                                var logValues = R.EvaluateNumericVector("kernel$x");
                                xKernel[item.Key] = logValues.Select(c => Math.Exp(c)).ToList();
                                yKernel[item.Key] = R.EvaluateNumericVector("kernel$y");
                                maximumY = Math.Max(maximumY, yKernel[item.Key].Max());
                                numberOfValuesRef = Math.Max(numberOfValuesRef, item.Value.x.Count);
                            }
                        } finally {
                        }
                    }
                }
            }
            return (yKernel, xKernel, maximumY, numberOfValuesRef);
        }

        /// <summary>
        /// Compute envelope based on kernel
        /// </summary>
        /// <param name="values"></param>
        /// <param name="name"></param>
        /// <param name="xKernel"></param>
        /// <param name="yKernel"></param>
        /// <param name="paletteColor"></param>
        /// <param name="counter"></param>
        /// <param name="maximumY"></param>
        /// <param name="numberOfValuesRef"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        protected AreaSeries CreateEnvelope(
            List<double> values,
            string name,
            Dictionary<string, List<double>> yKernel,
            Dictionary<string, List<double>> xKernel,
            OxyColor paletteColor,
            int counter,
            double maximumY,
            double numberOfValuesRef,
            bool horizontal,
            bool equalSize
        ) {
            var scaling = equalSize ? _scale : values.Count / numberOfValuesRef / maximumY * .5;
            var areaSeries = new AreaSeries() {
                Color = paletteColor,
                MarkerType = MarkerType.None,
                StrokeThickness = .5,
            };

            if (horizontal) {
                for (int i = 0; i < xKernel[name].Count; i++) {
                    areaSeries.Points.Add(new DataPoint(xKernel[name][i], yKernel[name][i] * scaling + counter));
                    areaSeries.Points2.Add(new DataPoint(xKernel[name][i], -yKernel[name][i] * scaling + counter));
                };
            } else {
                for (int i = 0; i < xKernel[name].Count; i++) {
                    areaSeries.Points.Add(new DataPoint(yKernel[name][i] * scaling + counter, xKernel[name][i]));
                    areaSeries.Points2.Add(new DataPoint(-yKernel[name][i] * scaling + counter, xKernel[name][i]));
                };
            }
            return areaSeries;
        }

        /// <summary>
        /// Create HorizontalBoxPlotSeries
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="paletteColor"></param>
        /// <param name="values"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        protected HorizontalBoxPlotSeries CreateHorizontalBoxPlotSerie(
            int counter,
            OxyColor paletteColor,
            List<double> values,
            double lowerBound = 5,
            double upperBound = 95
        ) {
            var series = new HorizontalBoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, paletteColor),
                StrokeThickness = .5,
                Stroke = OxyColors.Black,
                WhiskerWidth = .05,
                BoxWidth = .05,
            };
            var boxPlotItem = createBoxPlotItem(counter, values, lowerBound, upperBound);
            series.Items.Add(boxPlotItem);
            return series;
        }

        /// <summary>
        /// Create vertical BoxPlotSeries
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="paletteColor"></param>
        /// <param name="values"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        protected BoxPlotSeries CreateBoxPlotSerie(
            int counter,
            OxyColor paletteColor,
            List<double> values,
            double lowerBound = 5,
            double upperBound = 95
        ) {
            var series = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, paletteColor),
                StrokeThickness = .5,
                Stroke = OxyColors.Black,
                WhiskerWidth = .05,
                BoxWidth = .05,
            };
            var boxPlotItem = createBoxPlotItem(counter, values, lowerBound, upperBound);
            series.Items.Add(boxPlotItem);
            return series;
        }

        private BoxPlotItem createBoxPlotItem(int counter, List<double> values, double lowerBound, double upperBound) {
            var dp = asBoxPlotDataPoint(values, lowerBound: lowerBound, upperBound: upperBound, showOutliers: false);
            var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median, dp.UpperBox, dp.UpperWisker) {
                Outliers = dp.Outliers,
                Mean = values.Average(),
            };
            return boxPlotItem;
        }

        /// <summary>
        /// Create BoxplotItem (vertical)
        /// </summary>
        /// <param name="values"></param>
        /// <param name="paletteColor"></param>
        /// <param name="axis"></param>
        /// <param name="counter"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        protected BoxPlotSeries CreateBoxPlotItem(
            List<double> values,
            OxyColor paletteColor,
            LogarithmicAxis axis,
            int counter,
            double lowerBound,
            double upperBound,
            double minimum,
            double maximum
        ) {
            var series = CreateBoxPlotSerie(
                counter,
                paletteColor,
                values,
                lowerBound,
                upperBound
            );
            minimum = Math.Min(minimum, series.Items.First().LowerWhisker);
            maximum = Math.Max(maximum, series.Items.First().UpperWhisker);
            axis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            axis.MajorStep = axis.MajorStep > 0 ? axis.MajorStep : double.NaN;
            return series;
        }

        /// <summary>
        /// Create HorizontalBoxplotItem
        /// </summary>
        /// <param name="values"></param>
        /// <param name="paletteColor"></param>
        /// <param name="axis"></param>
        /// <param name="counter"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        protected HorizontalBoxPlotSeries CreateHorizontalBoxPlotItem(
            List<double> values,
            OxyColor paletteColor,
            LogarithmicAxis axis,
            int counter,
            double lowerBound,
            double upperBound,
            double minimum,
            double maximum
        ) {
            var series = CreateHorizontalBoxPlotSerie(
                counter,
                paletteColor,
                values,
                lowerBound,
                upperBound
            );
            minimum = Math.Min(minimum, series.Items.First().LowerWhisker);
            maximum = Math.Max(maximum, series.Items.First().UpperWhisker);
            axis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            axis.MajorStep = axis.MajorStep > 0 ? axis.MajorStep : double.NaN;
            return series;
        }

        /// <summary>
        /// Add mean to envelope
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="values"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        protected ScatterSeries CreateMeanSeries(int counter, List<double> values, bool horizontal) {
            var series = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Black,
                MarkerSize = 3,
            };
            if (horizontal) {
                series.Points.Add(new ScatterPoint(values.Average(), counter));
            } else {
                series.Points.Add(new ScatterPoint(counter, values.Average()));
            }
            return series;
        }

        /// <summary>
        /// Add line segments to envelope
        /// </summary>
        /// <param name="yKernel"></param>
        /// <param name="xKernel"></param>
        /// <param name="maximumY"></param>
        /// <param name="numberOfValuesRef"></param>
        /// <param name="counter"></param>
        /// <param name="values"></param>
        /// <param name="percentage"></param>
        /// <param name="horizontal"></param>
        /// <param name="equalSize"></param>
        /// <param name="axis"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        protected LineSeries CreatePercentileSeries(
            List<double> yKernel,
            List<double> xKernel,
            double maximumY,
            double numberOfValuesRef,
            int counter,
            List<double> values,
            double percentage,
            bool horizontal,
            bool equalSize,
            LogarithmicAxis axis,
            double minimum,
            double maximum
        ) {
            var percentile = values.Percentile(percentage);
            var scaling = equalSize ? _scale : values.Count / numberOfValuesRef / maximumY * .5;
            var lineSeries = new LineSeries() {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
                StrokeThickness = .5
            };
            minimum = Math.Min(minimum, percentile);
            maximum = Math.Max(maximum, percentile);
            axis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            axis.MajorStep = axis.MajorStep > 0 ? axis.MajorStep : double.NaN;
            var ix = xKernel.Count(c => c <= percentile);
            if (horizontal) {
                lineSeries.Points.Add(new DataPoint(percentile, yKernel[ix] * scaling + counter));
                lineSeries.Points.Add(new DataPoint(percentile, -yKernel[ix] * scaling + counter));
                return lineSeries;
            } else {
                lineSeries.Points.Add(new DataPoint(yKernel[ix] * scaling + counter, percentile));
                lineSeries.Points.Add(new DataPoint(-yKernel[ix] * scaling + counter, percentile));
                return lineSeries;
            }
        }
    }
}
