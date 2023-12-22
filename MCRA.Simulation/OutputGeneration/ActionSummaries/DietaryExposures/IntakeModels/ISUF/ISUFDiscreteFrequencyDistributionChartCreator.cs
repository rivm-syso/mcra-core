using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ISUFDiscreteFrequencyDistributionChartCreator : ReportHistogramChartCreatorBase {

        private ISUFModelResultsSection _section;

        public override string ChartId {
            get {
                var pictureId = "5828f371-483d-40c6-a627-80dbde3a0b44";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Discrete frequency distribution.";

        public ISUFDiscreteFrequencyDistributionChartCreator(ISUFModelResultsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override PlotModel Create() {
            return create(_section.DiscreteFrequencies);
        }

        private PlotModel create(List<double> discreteFrequencies) {
            var plotModel = createDefaultPlotModel();
            var shift = 1.0 / discreteFrequencies.Count;
            var left = 0d;

            var bins = new List<HistogramBin>();
            foreach (var item in discreteFrequencies) {
                bins.Add(new HistogramBin() {
                    Frequency = item,
                    XMinValue = left,
                    XMaxValue = left + shift,
                });
                left += shift;
            }

            var histogramSeries = createDefaultHistogramSeries(bins);
            histogramSeries.FillColor = OxyColors.Red;
            histogramSeries.StrokeColor = OxyColor.FromArgb(255, 232, 0, 0);
            plotModel.Series.Add(histogramSeries);

            var horizontalAxis = createLinearHorizontalAxis("proportion of days per individual");
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearVerticalAxis("proportion of individuals", 1);
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
