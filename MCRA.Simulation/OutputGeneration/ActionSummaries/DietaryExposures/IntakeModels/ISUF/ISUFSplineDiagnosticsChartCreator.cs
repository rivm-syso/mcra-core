using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ISUFSplineDiagnosticsChartCreator : ReportLineChartCreatorBase {

        private ISUFModelResultsSection _section;

        public ISUFSplineDiagnosticsChartCreator(ISUFModelResultsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string Title => "Spline transformation of exposure amounts.";

        public override string ChartId {
            get {
                var pictureId = "a4cf4300-a833-4701-8721-fbae5759a76a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var z = _section.ISUFDiagnostics.Select(c => c.Z).ToList();
            var gz = _section.ISUFDiagnostics.Select(c => c.GZ).ToList();
            var transformedDailyIntakes = _section.ISUFDiagnostics.Select(c => c.TransformedDailyIntakes).ToList();

            var yTitle = string.Empty;
            if (_section.Power == 0) {
                yTitle = "Log transformed exposure amounts";
            } else if (_section.Power == 1) {
                yTitle = "Untransformed (identical) exposure amounts";
            } else {
                yTitle = $"Power transformed ({_section.Power:F2}) exposure amounts";
            }
            return create(z, gz, transformedDailyIntakes, yTitle);
        }

        private PlotModel create(List<double> z, List<double> gz, List<double> transformedDailyIntakes, string yTitle) {
            var plotModel = createDefaultPlotModel();
            var series1 = new LineSeries() {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
            };
            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 1.5,
            };
            for (int i = 0; i < z.Count; i++) {
                series1.Points.Add(new DataPoint(z[i], gz[i]));
                series2.Points.Add(new ScatterPoint(z[i], transformedDailyIntakes[i]));
            }
            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            var horizontalAxis = createLinearAxis("Theoretical residuals");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis(yTitle);
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
