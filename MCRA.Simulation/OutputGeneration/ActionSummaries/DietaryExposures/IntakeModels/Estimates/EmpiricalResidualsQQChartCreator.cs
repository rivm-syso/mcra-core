using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EmpiricalResidualsQQChartCreator : ReportLineChartCreatorBase {

        private NormalAmountsModelResidualSection _section;

        public override string Title => "Normal QQ-plot of observed residuals";

        public override string ChartId {
            get {
                var pictureId = "9c9d2339-a864-4e43-8e5a-227f68250d62";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public EmpiricalResidualsQQChartCreator(NormalAmountsModelResidualSection section) {
            Width = 500;
            Height = 350; ;
            _section = section;
        }

        public override PlotModel Create() {
            return create(_section.Residuals);
        }

        private PlotModel create(List<double> residuals) {
            residuals = residuals.Order().ToList();

            var series1 = new LineSeries() {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
            };
            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 1.5,
            };
            for (int i = 0; i < residuals.Count; i++) {
                var z = NormalDistribution.InvCDF(0, 1, ((i + 1) - 3D / 8D) / (residuals.Count + 1D / 4D));
                series1.Points.Add(new DataPoint(z, z));
                series2.Points.Add(new ScatterPoint(z, residuals[i]));
            }

            var plotModel = createDefaultPlotModel(string.Empty);

            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            var horizontalAxis = createLinearAxis("Theoretical residuals");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("Observed residuals");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
