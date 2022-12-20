using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotHistogramCreator : OxyPlotChartCreator {

        /// <summary>
        /// Creates a default histogram series with default settings for, e.g., fill
        /// color and stroke color based on the specified bins.
        /// </summary>
        /// <param name="bins"></param>
        /// <returns></returns>
        protected static HistogramSeries createDefaultHistogramSeries(List<HistogramBin> bins) {
            var histogramSeries = new HistogramSeries {
                FillColor = OxyColors.CornflowerBlue,
                StrokeColor = OxyColor.FromArgb(255, 78, 132, 233),
                Items = bins
            };
            return histogramSeries;
        }

        /// <summary>
        /// Logarithmic axis: 
        /// MajorGridlineStyle = LineStyle.Dash,
        /// MinorGridlineStyle = LineStyle.None,
        /// MinorTickSize = 0,
        /// Position = AxisPosition.Bottom,
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        protected virtual LogarithmicAxis createLog10HorizontalAxis(string title) {
            return new LogarithmicAxis() {
                Title = title,
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
            };
        }

        /// <summary>
        /// Logarithmic axis: 
        /// MajorGridlineStyle = LineStyle.Dash,
        /// MinorGridlineStyle = LineStyle.None,
        /// MinorTickSize = 0,
        /// Position = AxisPosition.Bottom,
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        protected virtual LinearAxis createLinearHorizontalAxis(string title) {
            return new LinearAxis() {
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
                Title = title,
            };
        }

        /// <summary>
        /// Linear axis:
        /// MajorGridlineStyle = LineStyle.Dash,
        /// MinorGridlineStyle = LineStyle.None,
        /// MinorTickSize = 0,
        /// Position = AxisPosition.Left,
        /// </summary>
        /// <param name="title"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        protected virtual LinearAxis createLinearVerticalAxis(string title, double maximum = double.NaN) {
            return new LinearAxis() {
                Title = title,
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = maximum,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
            };
        }

        /// <summary>
        /// Create a reference histogram series
        /// </summary>
        /// <param name="bins"></param>5
        /// <param name="referenceDose"></param>
        /// <param name="smaller"></param>
        /// <returns></returns>
        protected static HistogramSeries createReferenceSeries(
            List<HistogramBin> bins, 
            double referenceDose, 
            bool smaller = false
        ) {
            List<HistogramBin> referenceBins = null;
            if (smaller) {
                referenceBins = bins.Where(c => c.XMinValue < referenceDose)
                    .Select(c => new HistogramBin() {
                        Frequency = c.Frequency,
                        XMaxValue = c.XMaxValue,
                        XMinValue = c.XMinValue
                    }).ToList();
                if (referenceBins.Last().XMaxValue > referenceDose) {
                    referenceBins.Last().XMaxValue = referenceDose;
                }
            } else {
                referenceBins = bins.Where(c => c.XMaxValue > referenceDose)
                    .Select(c => new HistogramBin() {
                        Frequency = c.Frequency,
                        XMaxValue = c.XMaxValue,
                        XMinValue = c.XMinValue
                    }).ToList();
                if (referenceBins.First().XMinValue < referenceDose) {
                    referenceBins.First().XMinValue = referenceDose;
                }
            }

            var histogramSeries = new HistogramSeries() {
                FillColor = OxyColors.Red,
                StrokeColor = OxyColor.FromArgb(255, 232, 0, 0),
                Items = referenceBins,
            };
            return histogramSeries;
        }

        protected static double getSmartInterval(double min, double max, int maxSteps) {
            if (min > max) {
                (max, min) = (min, max);
            }
            var totalInterval = max - min;
            var ticks = new int[] { 1, 2, 5 };
            var tickIndex = ticks.Length - 1;
            var powerIndex = BMath.Ceiling(Math.Log10(max - min));
            var interval = ticks[tickIndex] * Math.Pow(10, powerIndex);
            while ((totalInterval / interval) < maxSteps) {
                if (tickIndex == 0) {
                    tickIndex = ticks.Length;
                    powerIndex--;
                }
                tickIndex--;
                interval = ticks[tickIndex] * Math.Pow(10, powerIndex);
            }

            tickIndex++;
            if (tickIndex == ticks.Length) {
                powerIndex++;
                tickIndex = 0;
            }
            interval = ticks[tickIndex] * Math.Pow(10, powerIndex);
            return interval;
        }
    }
}
