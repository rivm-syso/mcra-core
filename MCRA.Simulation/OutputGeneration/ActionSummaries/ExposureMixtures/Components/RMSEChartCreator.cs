using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RMSEChartCreator : ReportLineChartCreatorBase {

        private ComponentDiagnosticsSection _section;

        public override string Title => "Difference in RMSE ratio as a function of the number of components (green line: first optimum, purple line: second optimum).";

        public override string ChartId {
            get {
                var pictureId = "8663e636-af04-4c48-b354-07009f7eb380";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public RMSEChartCreator(ComponentDiagnosticsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override PlotModel Create() {
            var ratio = _section.RRMSEdifference;

            var yTitle = "Ratio RMSE";
            return create(ratio, yTitle);
        }

        private PlotModel create(List<double> ratio, string yTitle) {
            var optMix = ratio.OrderByDescending(x => x).ToList();
            var plotModel = createDefaultPlotModel();
            var series1 = new LineSeries() {
                Color = OxyColors.Green,
                MarkerType = MarkerType.None,
            };
            var series2 = new LineSeries() {
                Color = OxyColors.Purple,
                MarkerType = MarkerType.None,
            };
            var series3 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 5,
            };
            for (int i = 0; i < ratio.Count; i++) {
                series3.Points.Add(new ScatterPoint(i + 2, ratio[i]));
            }
            series1.Points.Add(new DataPoint(1, optMix[0]));
            series1.Points.Add(new DataPoint(ratio.Count + 1.5, optMix[0]));

            series2.Points.Add(new DataPoint(1, optMix[1]));
            series2.Points.Add(new DataPoint(ratio.Count + 1.5, optMix[1]));

            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);
            plotModel.Series.Add(series3);

            var horizontalAxis = createLinearAxis("Number of components (k)");
            horizontalAxis.Position = AxisPosition.Bottom;
            horizontalAxis.MajorGridlineStyle = LineStyle.None;
            horizontalAxis.MinorGridlineStyle = LineStyle.None;
            horizontalAxis.MajorStep = 1;
            horizontalAxis.MinorStep = 1;
            horizontalAxis.Minimum = 1.5;
            horizontalAxis.Maximum = ratio.Count + 1.5;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis(yTitle);
            plotModel.Axes.Add(verticalAxis);
            return plotModel;
        }
    }
}
