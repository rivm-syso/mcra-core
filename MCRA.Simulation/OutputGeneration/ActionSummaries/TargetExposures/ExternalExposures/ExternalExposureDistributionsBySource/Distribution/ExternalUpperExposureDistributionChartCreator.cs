using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalUpperExposureDistributionChartCreator : ReportHistogramChartCreatorBase {

        private ExternalUpperExposureDistributionSection _section;
        private string _exposureUnit;

        public ExternalUpperExposureDistributionChartCreator(ExternalUpperExposureDistributionSection section, string exposureUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _exposureUnit = exposureUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "3b117e26-bf04-4545-b102-b24554d05c7a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                _exposureUnit
            );
        }

        private PlotModel create(
            List<HistogramBin> binsTransformed,
            string intakeUnit
        ) {
            var bins = binsTransformed
                .Select(r => new HistogramBin() {
                    Frequency = r.Frequency,
                    XMinValue = Math.Pow(10, r.XMinValue),
                    XMaxValue = Math.Pow(10, r.XMaxValue),
                })
                .ToList();

            var title = $"Transformed upper external exposure distribution {_section.UpperPercentage:F1}%";
            var plotModel = createDefaultPlotModel(title);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var yHigh = histogramSeries.Items.Select(c => c.Frequency).Max() * 1.1;
            var horizontalAxis = createLog10HorizontalAxis(title: $"Exposure ({intakeUnit})");
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearVerticalAxis("Frequency", yHigh);
            plotModel.Axes.Add(verticalAxis);
            return plotModel;
        }
    }
}
