using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UntransformedTotalIntakeDistributionChartCreator : ReportHistogramChartCreatorBase {

        private UntransformedTotalIntakeDistributionSection _section;
        private string _intakeUnit;

        public override string ChartId {
            get {
                var pictureId = "b5469184-7d90-4144-a575-a3ec11e71357";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Untransformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public UntransformedTotalIntakeDistributionChartCreator(UntransformedTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override PlotModel Create() {
            return create(_section.IntakeDistributionBins, _intakeUnit);
        }

        private PlotModel create(List<HistogramBin> bins, string intakeUnit) {
            var plotModel = createDefaultPlotModel();

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            if (bins.Any(r => !double.IsNaN(r.Frequency))) {
                var yHigh = histogramSeries.Items.Select(c => c.Frequency).Max() * 1.1;

                var horizontalAxis = createLinearHorizontalAxis($"Exposure ({intakeUnit})");
                plotModel.Axes.Add(horizontalAxis);

                var verticalAxis = createLinearVerticalAxis("Frequency", yHigh);
                plotModel.Axes.Add(verticalAxis);
            }

            return plotModel;
        }
    }
}
