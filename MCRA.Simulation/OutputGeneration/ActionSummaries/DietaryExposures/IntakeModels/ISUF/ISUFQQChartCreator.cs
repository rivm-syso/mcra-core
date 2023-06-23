using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ISUFQQChartCreator : OxyPlotLineCreator {

        private ISUFModelResultsSection _section;

        public ISUFQQChartCreator(ISUFModelResultsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "0fe59548-9212-4581-8e69-45463869ec4a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Normal QQ-plot of transformed exposure amounts.";

        public override PlotModel Create() {
            var z = _section.ISUFDiagnostics.Select(c => c.Z).ToList();
            var zhat = _section.ISUFDiagnostics.Select(c => c.Zhat).ToList();
            return create(z, zhat);
        }

        private PlotModel create(List<double> z, List<double> zhat) {
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
                series1.Points.Add(new DataPoint(z[i], z[i]));
                series2.Points.Add(new ScatterPoint(z[i], zhat[i]));
            }

            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            var horizontalAxis = createLinearAxis("theoretical residuals");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("observed residuals");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
