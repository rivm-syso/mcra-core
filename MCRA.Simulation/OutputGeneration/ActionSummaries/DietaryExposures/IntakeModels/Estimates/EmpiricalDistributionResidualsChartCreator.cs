using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EmpiricalDistributionResidualsChartCreator : ReportHistogramChartCreatorBase {

        private NormalAmountsModelResidualSection _section;

        public EmpiricalDistributionResidualsChartCreator(NormalAmountsModelResidualSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string Title => "Empirical distribution of the observed residuals";

        public override string ChartId {
            get {
                var pictureId = "998886ac-898f-4e73-8d00-2ecdc933881a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.Residuals);
        }

        private PlotModel create(List<double> residuals) {
            var minimum = residuals.Min();
            var maximum = residuals.Max();
            int numberOfBins = Math.Sqrt(residuals.Count) < 100 ? BMath.Ceiling(Math.Sqrt(residuals.Count)) : 100;
            var bins = residuals.MakeHistogramBins(numberOfBins, minimum, maximum);

            var plotModel = createDefaultPlotModel(string.Empty);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var series = base.getNormalDistributionSeries(bins, 0, 1);
            plotModel.Series.Add(series);

            var horizontalAxis = createLinearHorizontalAxis("Observed residuals");
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearVerticalAxis("Frequency");
            plotModel.Axes.Add(verticalAxis);
            return plotModel;
        }

       
    }
}
