using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public sealed class HistogramChartCreator : OxyPlotHistogramCreator {

        private readonly List<double> _values;
        private readonly string _title;
        private readonly bool _isLogarithmic;
        private readonly string _titleX;
        private readonly string _titleY;

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
            return create(
                _values,
                null,
                _title,
                _isLogarithmic,
                _titleX,
                _titleY
            );
        }

        private static PlotModel create(
            List<double> values,
            List<double> weights = null,
            string title = null,
            bool isLogarithmic = false,
            string titleX = null,
            string titleY = null
        ) {
            var plotModel = createDefaultPlotModel(title);

            weights = weights ?? Enumerable.Repeat(1D, values.Count).ToList();
            var bins = isLogarithmic
                ? values
                    .Select(r => Math.Log10(r))
                    .MakeHistogramBins(weights)
                    .Select(r => new HistogramBin() {
                        Frequency = r.Frequency,
                        XMinValue = Math.Pow(10, r.XMinValue),
                        XMaxValue = Math.Pow(10, r.XMaxValue),
                    })
                    .ToList()
                : values.MakeHistogramBins(weights);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var yHigh = bins.Select(c => c.Frequency).Max() * 1.1;
            var verticalAxis = createLinearVerticalAxis(titleY);
            plotModel.Axes.Add(verticalAxis);

            if (isLogarithmic) {
                var horizontalAxis = createLog10HorizontalAxis(titleX);
                plotModel.Axes.Add(horizontalAxis);
            } else {
                var horizontalAxis = createLinearHorizontalAxis(titleY);
                plotModel.Axes.Add(horizontalAxis);
            }
            return plotModel;
        }
    }
}
