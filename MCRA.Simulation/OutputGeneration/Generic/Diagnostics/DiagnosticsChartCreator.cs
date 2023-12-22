using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DiagnosticsChartCreator : ReportLineChartCreatorBase {

        private List<SigmaSizeRecord> _mcSigmas;
        private List<SigmaSizeRecord> _uncertaintySigmas;
        private double _percentage;
        private int _bootstrapSize;

        public DiagnosticsChartCreator(
            List<SigmaSizeRecord> mcSigmas,
            int height,
            int width,
            double percentage,
            int bootstrapSize,
            List<SigmaSizeRecord> uncertaintySigmas = null
        ) {
            Width = width;
            Height = height;
            _mcSigmas = mcSigmas;
            _uncertaintySigmas = uncertaintySigmas;
            _percentage = percentage;
            _bootstrapSize = bootstrapSize;
        }

        public override string Title => "Variability diagnostics.";

        public override string ChartId {
            get {
                var pictureId = "996735b5-8362-4376-92ff-07a40cd64a3f";
                return StringExtensions.CreateFingerprint(_percentage + pictureId);
            }
        }

        public override PlotModel Create() {
            return create("Diagnostics");
        }

        private PlotModel create(string title) {
            var mcResults = _mcSigmas.Where(c => c.Percentage == _percentage).ToList();
            var plotModel = new PlotModel();
            var series1 = new LineSeries() {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
                StrokeThickness = 1

            };
            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 3,
            };

            foreach (var item in mcResults) {
                series1.Points.Add(new DataPoint(item.Size, item.Sigma));
                series2.Points.Add(new ScatterPoint(item.Size, item.Sigma));
                var df = item.NumberOfValues - 1;
                var lower = Math.Sqrt(df * Math.Pow(item.Sigma, 2) / ChiSquaredDistribution.InvCDF(df, 0.95));
                var upper = Math.Sqrt(df * Math.Pow(item.Sigma, 2) / ChiSquaredDistribution.InvCDF(df, 0.05));
                var scatterErrorSeries = new ScatterErrorSeries() {
                    MarkerType = MarkerType.None,
                    ErrorBarColor = OxyColors.Red,
                };
                scatterErrorSeries.Points.Add(asymmetricErrorsBars(upper, lower, item.Size));
                if (df > 1) {
                    plotModel.Series.Add(scatterErrorSeries);
                }
            }

            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            if (_uncertaintySigmas != null && _uncertaintySigmas.All(c => !double.IsNaN(c.Sigma))) {
                var series3 = new LineSeries() {
                    Color = OxyColors.Black,
                    MarkerType = MarkerType.None,
                    StrokeThickness = 1
                };
                var series4 = new ScatterSeries() {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColors.Blue,
                    MarkerSize = 3,
                };
                var series5 = new ScatterSeries() {
                    MarkerType = MarkerType.Square,
                    MarkerFill = OxyColors.Transparent,
                    MarkerSize = 5,
                    MarkerStroke = OxyColors.Red
                };
                var bootstrapVariances = _uncertaintySigmas.Where(c => c.Percentage == _percentage).ToList();

                foreach (var item in bootstrapVariances) {
                    series3.Points.Add(new DataPoint(item.Size, item.Sigma));
                    series4.Points.Add(new ScatterPoint(item.Size, item.Sigma));
                }
                //plotModel.Series.Add(series3);
                plotModel.Series.Add(series4);
                var bootstrapVariance = _uncertaintySigmas.Single(c => c.Percentage == _percentage && c.Size == _bootstrapSize).Sigma;
                series5.Points.Add(new ScatterPoint(_bootstrapSize, bootstrapVariance));
                plotModel.Series.Add(series5);
            }

            var horizontalAxis = new LogarithmicAxis() {
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
                Title = "Sample size",
                Base = 2,
            };

            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Left,
                Title = $"Stdev of estimated p{_percentage}",
            };
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }

        private ScatterErrorPoint asymmetricErrorsBars(double upper, double lower, int x) {
            var error = (upper - lower);
            var y = upper - error * 0.5;
            return new ScatterErrorPoint(x, y, 0, error);
        }
    }
}
