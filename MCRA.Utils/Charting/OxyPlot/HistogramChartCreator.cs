using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Core.Drawing;

namespace MCRA.Utils.Charting.OxyPlot {
    public class HistogramChartCreator : OxyPlotHistogramCreator {

        private readonly List<double> _values;
        private readonly string _title;
        private readonly bool _isLogarithmic;
        private readonly string _titleX;
        private readonly string _titleY;

        public override string ChartId => "8ABCBE21-E00B-4C9F-868A-7E1C2D9C3652";

        public HistogramChartCreator(
            List<double> values, 
            string title,
            bool isLogarithmic = false,
            string titleX = null,
            string titleY = null
        ) {
            _values = values;
            _title = title;
            _isLogarithmic = isLogarithmic;
            _titleX = titleX;
            _titleY = titleY;
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel(_title);

            var weights = Enumerable.Repeat(1D, _values.Count);
            var bins = _isLogarithmic
                ? _values
                    .Select(r => Math.Log10(r))
                    .MakeHistogramBins(weights)
                    .Select(r => new HistogramBin() {
                        Frequency = r.Frequency,
                        XMinValue = Math.Pow(10, r.XMinValue),
                        XMaxValue = Math.Pow(10, r.XMaxValue),
                    })
                    .ToList()
                : _values.MakeHistogramBins(weights);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var yHigh = bins.Select(c => c.Frequency).Max() * 1.1;
            var verticalAxis = createLinearVerticalAxis(_titleY);
            plotModel.Axes.Add(verticalAxis);

            if (_isLogarithmic) {
                var horizontalAxis = createLog10HorizontalAxis(_titleX);
                plotModel.Axes.Add(horizontalAxis);
            } else {
                var horizontalAxis = createLinearHorizontalAxis(_titleY);
                plotModel.Axes.Add(horizontalAxis);
            }
            return plotModel;
        }
    }
}
