using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class BoxPlotChartCreatorBase : OxyPlotBoxPlotCreator, IReportChartCreator {

        public abstract string ChartId { get; }

        public virtual string Title { get; }

        /// <summary>
        /// Converts the target uncertain datapoint to a boxplot datapoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="wiskerType">Calculationmethod for the wiskers</param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="interQuartileRangeFactor"></param>
        /// <returns></returns>
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
        /// <param name="source"></param>
        /// <param name="wiskerType">Calculationmethod for the wiskers</param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="interQuartileRangeFactor"></param>
        /// <returns></returns>
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
        /// <param name="whiskers"></param>
        /// <param name="outliers"></param>
        /// <param name="x">The position in the graph</param>
        /// <param name="replace"></param>
        /// <param name="lowerBound"></param>
        /// <param name="showOutliers"></param>
        /// <returns></returns>
        protected MultipleWhiskerBoxPlotItem setSeries(
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
        }
    }
}
