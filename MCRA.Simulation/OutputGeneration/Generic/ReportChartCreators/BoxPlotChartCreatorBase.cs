using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class BoxPlotChartCreatorBase : OxyPlotBoxPlotCreator, IReportChartCreator {

        protected const int _cellSize = 20;

        public OxyColor BoxColor { get; set; } = OxyColors.CornflowerBlue;

        public OxyColor StrokeColor { get; set; } = OxyColors.Blue;

        public abstract string ChartId { get; }

        public virtual string Title { get; }

        protected PlotModel create(
            ICollection<BoxPlotChartRecord> records,
            string labelHorizontalAxis,
            bool showOutliers,
            bool showLabels = true,
            int outliersLimit = 100
        ) {
            var recordsReversed = records.Where(c => c.Percentage > 0).Reverse();
            var minima = records.Where(r => r.MinPositives > 0).Select(r => r.MinPositives).ToList();
            var minimum = minima.Any() ? minima.Min() * 0.9 : 1e-8;

            var plotModel = createDefaultPlotModel();

            // Create boxplot series
            var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, BoxColor),
                StrokeThickness = 1,
                Stroke = StrokeColor,
                BoxWidth = .4,
                WhiskerWidth = 1.1,
            };
            var maximum = double.NegativeInfinity;
            var xOrder = 0;
            foreach (var item in recordsReversed) {
    
                var outliers = new List<double>();
                if (showOutliers) {
                    if (item.Outliers?.Count > outliersLimit) {
                        // Restrict the number of outliers to specified limit
                        outliers = item.Outliers.Take(outliersLimit - 2).ToList();
                        // Including the minimum and maximum of the outliers
                        outliers.Add(item.Outliers.Min());
                        outliers.Add(item.Outliers.Max());
                    } else {
                        outliers = item.Outliers;
                    }
                }

                var whiskers = getWhiskers(item.P5, item.P10, item.P25, item.P50, item.P75, item.P90, item.P95);
                var percentiles = item.Percentiles.Where(c => !double.IsNaN(c)).ToList();
                var replace = percentiles.Any() ? percentiles.Min() : 0;
                var boxPlotItem = createBoxPlotItem(whiskers, outliers, xOrder, replace, 0, showOutliers);
                series.Items.Add(boxPlotItem);
                maximum = Math.Max(maximum, double.IsNaN(item.P95) ? maximum : item.P95);
                xOrder++;
            };
            plotModel.Series.Add(series);

            // Create horizontal logarithmic axis
            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = labelHorizontalAxis,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
                MajorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 2
            };
            updateLogarithmicAxis(logarithmicAxis, minimum, maximum);
            plotModel.Axes.Add(logarithmicAxis);

            // Create vertical category axis
            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                Position = AxisPosition.Left
            };
            if (showLabels) {
                foreach (var item in recordsReversed) {
                    categoryAxis.Labels.Add(item.GetLabel());
                }
            }
            plotModel.Axes.Add(categoryAxis);

            return plotModel;
        }

        /// <summary>
        /// Converts the target uncertain datapoint to a boxplot datapoint.
        /// </summary>
        protected BoxPlotDataPoint asBoxPlotDataPoint<T>(
            UncertainDataPoint<T> source,
            WiskerType wiskerType = WiskerType.ExtremePercentiles,
            double lowerBound = 2.5,
            double upperBound = 97.5,
            double interQuartileRangeFactor = 1.5
        ) {
            var lowerBox = source.UncertainValues.Percentile(25);
            var upperBox = source.UncertainValues.Percentile(75);
            double lowerWisker = 0d;
            double upperWisker = 0d;

            switch (wiskerType) {
                case WiskerType.ExtremePercentiles:
                    lowerWisker = source.UncertainValues.Percentile(lowerBound);
                    upperWisker = source.UncertainValues.Percentile(upperBound);
                    break;
                case WiskerType.BasedOnInterQuartileRange:
                    var wiskerSize = interQuartileRangeFactor * (upperBox - lowerBox);
                    lowerWisker = lowerBox - wiskerSize;
                    upperWisker = upperBox + wiskerSize;
                    var valuesWithinRange = source.UncertainValues.Where(p => p >= lowerWisker && p <= upperWisker);
                    lowerWisker = valuesWithinRange.Min();
                    upperWisker = valuesWithinRange.Max();
                    break;
            }

            var boxPlotdataPoint = new BoxPlotDataPoint() {
                LowerWisker = lowerWisker,
                UpperWisker = upperWisker,
                LowerBox = lowerBox,
                UpperBox = upperBox,
                Median = source.UncertainValues.Percentile(50),
                Reference = source.ReferenceValue,
                Outliers = source.UncertainValues.Where(v => v > upperWisker || v < lowerWisker).ToList(),
            };
            return boxPlotdataPoint;
        }

        /// <summary>
        /// Converts the target uncertain datapoint to a boxplot datapoint.
        /// </summary>
        protected BoxPlotDataPoint asBoxPlotDataPoint(
            IEnumerable<double> source,
            WiskerType wiskerType = WiskerType.ExtremePercentiles,
            double lowerBound = 2.5,
            double upperBound = 97.5,
            double interQuartileRangeFactor = 1.5
        ) {
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
                Outliers = source.Where(v => v > upperWisker || v < lowerWisker).ToList(),
            };
            return boxPlotdataPoint;
        }

        /// <summary>
        /// Whiskers in order:: 0: p5, 1: p10, 2: p25, 3: p50, 4: p75, 5: p90, 6: p95
        /// </summary>
        protected static MultipleWhiskerBoxPlotItem createBoxPlotItem(
            double[] whiskers,
            List<double> outliers,
            double x,
            double replace,
            double lowerBound,
            bool showOutliers
        ) {
            var boxPlotItem = new BoxPlotItem(
                x,
                double.IsNaN(whiskers[1]) ? replace : whiskers[1],
                double.IsNaN(whiskers[2]) ? replace : whiskers[2],
                double.IsNaN(whiskers[3]) ? replace : whiskers[3],
                double.IsNaN(whiskers[4]) ? replace : whiskers[4],
                double.IsNaN(whiskers[5]) ? replace : whiskers[5]
            );
            if (showOutliers && outliers != null) {
                boxPlotItem.Outliers = outliers;
            }
            var multipleWhiskerBoxPlotItem = new MultipleWhiskerBoxPlotItem(
                boxPlotItem,
                double.IsNaN(whiskers[0]) ? replace : whiskers[0],
                double.IsNaN(whiskers[6]) ? replace : whiskers[6],
                lowerBound
            );
            return multipleWhiskerBoxPlotItem;
        }

        protected double[] getWhiskers(double p5, double p10, double p25, double p50, double p75, double p90, double p95) {
            var whiskers = new double[7];
            whiskers[0] = p5;
            whiskers[1] = p10;
            whiskers[2] = p25;
            whiskers[3] = p50;
            whiskers[4] = p75;
            whiskers[5] = p90;
            whiskers[6] = p95;
            for (int i = 1; i < whiskers.Length; i++) {
                if (whiskers[i - 1] > whiskers[i]) {
                    throw new Exception("Sorry, wrong order of percentiles, do your job");
                }
            }
            return whiskers;
        }

        protected void updateLogarithmicAxis(LogarithmicAxis logarithmicAxis, double minimum, double maximum) {
            logarithmicAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            logarithmicAxis.MajorStep = logarithmicAxis.MajorStep > 0 ? logarithmicAxis.MajorStep : double.NaN;
            logarithmicAxis.Minimum = minimum * .9;
            logarithmicAxis.AbsoluteMinimum = minimum * .9;
            logarithmicAxis.Maximum = 10 * maximum;
        }
    }
}
