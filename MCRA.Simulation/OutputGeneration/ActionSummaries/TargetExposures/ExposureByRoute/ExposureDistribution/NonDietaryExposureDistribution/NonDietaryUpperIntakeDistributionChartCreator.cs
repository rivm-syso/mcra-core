using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryUpperIntakeDistributionChartCreator : ReportHistogramChartCreatorBase {

        private NonDietaryUpperIntakeDistributionSection _section;
        private string _intakeUnit;

        public NonDietaryUpperIntakeDistributionChartCreator(NonDietaryUpperIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
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
                _intakeUnit
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

            var title = "Transformed upper non-dietary exposure distribution";
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
