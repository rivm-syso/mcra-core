using System;
using System.Linq;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CompoundExposureDistributionChartCreatorBase : HistogramChartCreatorBase {

        public PlotModel Create(CompoundExposureDistributionRecord record, string title, string unit, double maximum, double minimum, bool showTitle) {
            return Create(record, title, unit, maximum, minimum, double.NaN, showTitle);
        }

        public PlotModel Create(CompoundExposureDistributionRecord record, string title, string unit, bool showTitle) {
            return Create(record, title, unit, double.NaN, double.NaN, double.NaN, showTitle);
        }

        /// <summary>
        /// Exposures are natural logarithm transformed, mu and sigma are calculated on the natural logarithm scale
        /// The plot is on the log10 scale
        /// </summary> See also concentrationmodels graphs
        /// <param name="record"></param>
        /// <param name="unit"></param>
        /// <param name="maximum"></param>
        /// <param name="minimum"></param>
        /// <param name="globalMaximumFrequency"></param>
        /// <param name="showTitle"></param>
        /// <returns></returns>
        public PlotModel Create(
            CompoundExposureDistributionRecord record,
            string title,
            string unit,
            double maximum,
            double minimum,
            double globalMaximumFrequency,
            bool showTitle
        ) {
            var plotModel = new PlotModel {
                PlotMargins = new OxyThickness(50, 0, 0, 20)
            };

            var fractionTrueZero = record.Percentage / 100;
            var fractionPositives = (100 - record.Percentage) / 100;
            if (showTitle) {
                plotModel.Title = title;
                plotModel.TitleFontSize = 11;
                plotModel.TitleFontWeight = 200;
            }

            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                MinorTickSize = 0,
                UseSuperExponentialFormat = false,
                MajorTickSize = 4,
                MajorGridlineStyle = LineStyle.Dash,
                FontSize = 9,
                Angle = 45,
                TitleFontWeight = 200,
            };
            plotModel.Axes.Add(logarithmicAxis);

            var bins = record.HistogramBins
                .Select(r => new HistogramBin() {
                    Frequency = r.Frequency,
                    XMinValue = Math.Exp(r.XMinValue),
                    XMaxValue = Math.Exp(r.XMaxValue),
                })
                .ToList();

            if (double.IsNaN(minimum)) {
                maximum = bins.GetMaxBound() + bins.AverageBinSize();
                minimum = bins.GetMinBound();
            }
            logarithmicAxis.Minimum = minimum;
            logarithmicAxis.Maximum = maximum;

            var localMaximumFrequency = bins.Select(c => c.Frequency).Max();
            var rescaleZeroBar = 1d;
            if (!double.IsNaN(globalMaximumFrequency)) {
                rescaleZeroBar = localMaximumFrequency / globalMaximumFrequency;
                localMaximumFrequency = globalMaximumFrequency;
            }

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var intervalSteps = (bins.Count > 4) ? 7 : bins.Count + 2;
            var intervalX = getSmartInterval(minimum, maximum, intervalSteps);
            var intervalY = getSmartInterval(0d, localMaximumFrequency, 8);
            intervalY = (intervalY > 1) ? intervalY : 1;
            var totalArea = record.HistogramBins.Sum(b => b.Width * b.Frequency);
            logarithmicAxis.MajorStep = intervalX;

            var linearAxis = createLinearVerticalAxis(string.Empty, 1.1 * localMaximumFrequency);
            linearAxis.MajorTickSize = 2;
            linearAxis.FontSize = 9;
            linearAxis.TitleFontWeight = 200;
            linearAxis.MajorStep = intervalY;
            plotModel.Axes.Add(linearAxis);

            var mu = (double)record.Mu;
            var sigma = (double)record.Sigma;

            var fitSeries = new LineSeries() {
                Color = OxyColors.Black,
                XAxisKey = "normalDensity",
                StrokeThickness = 0.8,
            };
            var normalDensityAxis = new LinearAxis() {
                Key = "normalDensity",
                Position = AxisPosition.Top,
                IsAxisVisible = false,
                Minimum = Math.Log(minimum),
                Maximum = Math.Log(maximum),
            };
            plotModel.Axes.Add(normalDensityAxis);

            var fts = !double.IsNaN(fractionTrueZero) ? fractionTrueZero : 0;
            totalArea = (1 - fts) * totalArea / fractionPositives;

            var normalDensity = GriddingFunctions.Arange(Math.Log(minimum), Math.Log(maximum), 500)
                .Select(v => (x: v, y: NormalDistribution.PDF(mu, sigma, v) * totalArea))
                .ToList();

            foreach (var item in normalDensity) {
                fitSeries.Points.Add(new DataPoint(item.x, item.y));
            }
            plotModel.Series.Add(fitSeries);

            var maximumY1 = normalDensity.Select(c => c.y).Max();
            if (maximumY1 / logarithmicAxis.MajorStep > 6) {
                logarithmicAxis.MajorStep = maximumY1 / 6;
            }
            if (fractionTrueZero > 0) {
                plotModel = base.createZeroBarsAxis(plotModel, fractionTrueZero, rescaleZeroBar);
            }
            return plotModel;
        }
    }
}
