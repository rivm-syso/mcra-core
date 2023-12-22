using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;

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
    }
}
