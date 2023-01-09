using MCRA.Utils.Statistics;
using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public enum WiskerType {
        ExtremePercentiles,
        BasedOnInterQuartileRange,
    }

    public class ViolinChartCreatorBase : OxyPlotBoxPlotCreator {

        public ViolinChartCreatorBase() {
        }

        public override string ChartId {
            get { throw new NotImplementedException(); }
        }

        public override PlotModel Create() {
            throw new NotImplementedException();
        }

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
            IEnumerable<double> source,
            WiskerType wiskerType = WiskerType.ExtremePercentiles,
            double lowerBound = 2.5,
            double upperBound = 97.5,
            double interQuartileRangeFactor = 1.5
        ) {
            var lowerBox = source.Percentile(25);
            var upperBox = source.Percentile(75);
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
                Reference = source.Average(),
                Outliers = source.Where(v => v > upperWisker || v < lowerWisker).ToList(),
            };
            return boxPlotdataPoint;
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
                Outliers = source.Where(v => v > upperWisker || v < lowerWisker).ToList(),
            };
            return boxPlotdataPoint;
        }

        protected class BoxPlotDataPoint {
            public double LowerWisker { get; set; }
            public double UpperWisker { get; set; }
            public double LowerBox { get; set; }
            public double UpperBox { get; set; }
            public double Median { get; set; }
            public double Reference { get; set; }
            public List<double> Outliers { get; set; }
        }
    }
}
