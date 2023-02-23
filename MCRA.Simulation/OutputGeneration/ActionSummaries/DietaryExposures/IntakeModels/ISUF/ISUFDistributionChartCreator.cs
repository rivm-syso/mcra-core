using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ISUFDistributionChartCreator : HistogramChartCreatorBase {

        private ISUFModelResultsSection _section;


        public ISUFDistributionChartCreator(ISUFModelResultsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "5e1d487e-666e-4d9c-a138-53ca9f74854c";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var zHat = _section.ISUFDiagnostics.Select(md => md.Zhat).ToList();
            var splinefit = _section.ISUFDiagnostics.All(d => !double.IsNaN(d.GZ));

            var title = string.Empty;
            if (double.IsNaN(_section.Power)) {
                if (splinefit) {
                    title = "Distribution of log spline transformed positive exposure amounts";
                } else {
                    title = "Distribution of log transformed positive exposure amounts";
                }
            } else if (_section.Power == 1) {
                if (splinefit) {
                    title = "Distribution of spline untransformed (identical) positive exposure amounts";
                } else {
                    title = "Distribution of untransformed (identical) positive exposure amounts";
                }

            } else {
                if (splinefit) {
                    title = $"Distribution of power ({_section.Power:F2}) spline transformed positive exposure amounts";
                } else {
                    title = $"Distribution of power ({_section.Power:F2}) transformed positive exposure amounts";
                }
            }

            return create(zHat, title);
        }

        private PlotModel create(List<double> zHat, string title) {
            var bins = zHat.MakeHistogramBins();

            var plotModel = createDefaultPlotModel(title);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);

            var series = getNormalDistributionSeries(bins, 0, 1);
            plotModel.Series.Add(series);

            var horizontalAxis = createLinearHorizontalAxis("Normalized exposure amounts") ;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearVerticalAxis("Frequency");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}


