using MCRA.Utils;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportHistogramChartCreatorBase : OxyPlotHistogramCreator, IReportChartCreator {

        public abstract string ChartId { get; }
        public virtual string Title { get; }

        public OxyColor FillColor { get; set; } = OxyColors.CornflowerBlue;
        public OxyColor StrokeColor { get; set; } = OxyColor.FromArgb(255, 78, 132, 233);

        protected PlotModel createPlotModel(
                List<HistogramBin> binsTransformed,
                string title,
                string xtitle,
                OxyColor? fillColor = null,
                OxyColor? strokeColor = null
            ) {
            if (binsTransformed == null) {
                return null;
            }

            var bins = binsTransformed
                .Select(r => new HistogramBin() {
                    Frequency = r.Frequency,
                    XMinValue = Math.Pow(10, r.XMinValue),
                    XMaxValue = Math.Pow(10, r.XMaxValue),
                })
                .ToList();

            var plotModel = createDefaultPlotModel(title);

            var histogramSeries = createDefaultHistogramSeries(bins);
            histogramSeries.FillColor = fillColor == null ? FillColor : (OxyColor)fillColor;
            histogramSeries.StrokeColor = fillColor == null ? StrokeColor : (OxyColor)strokeColor;

            plotModel.Series.Add(histogramSeries);

            var horizontalAxis = createLog10HorizontalAxis(xtitle);
            plotModel.Axes.Add(horizontalAxis);

            var yHigh = histogramSeries.Items.Select(c => c.Frequency).DefaultIfEmpty().Max() * 1.1;
            var verticalAxis = createLinearVerticalAxis("Frequency", yHigh);
            plotModel.Axes.Add(verticalAxis);
            return plotModel;
        }

        /// <summary>
        /// returns normal distribution series
        /// </summary>
        /// <param name="bins"></param>
        /// <returns></returns>
        protected LineSeries getNormalDistributionSeries(List<HistogramBin> bins, double mu, double stdDev) {
            var series = new LineSeries() { Color = OxyColors.Black, };
            var maximum = mu + 3 * stdDev;
            var minimum = mu - 3 * stdDev;
            var sum = bins.Sum(b => b.Width * b.Frequency);
            var normalDensity = GriddingFunctions.Arange(minimum, maximum, 200)
                .Select(v => (x: v, y: NormalDistribution.PDF(mu, stdDev, v) * sum))
                .ToList();
            for (int i = 0; i < normalDensity.Count; i++) {
                series.Points.Add(new DataPoint(normalDensity[i].x, normalDensity[i].y));
            }
            return series;
        }

        protected PlotModel createZeroBarsAxis(PlotModel plotModel, double fractionTrueZero, double rescale) {
            var zeroBar = new NonDetectBarsAxis() {
                Fraction = fractionTrueZero,
                Label = "zero",
                AxisDistance = 15,
                Height = Height,
                Rescale = rescale,
            };
            plotModel.Axes.Add(zeroBar);
            return plotModel;
        }
    }
}
