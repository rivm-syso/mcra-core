using System.Threading.Tasks.Dataflow;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Core.Drawing;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class ViolinCreator : ViolinChartCreatorBase {

        private IDictionary<string, List<double>> _data;
        private string _title;

        public void CreateToFile(PlotModel plotModel, string filename) {
            PngExporter.Export(plotModel, filename, 500, 350, OxyColors.White, 96);
        }

        public override string ChartId => throw new NotImplementedException();

        public ViolinCreator(IDictionary<string, List<double>> data, string title) {
            _data = data;
            _title = title; 
        }

        /// <summary>
        /// Kernel density estimation in R
        /// d < -density(data)
        /// https://r-charts.com/distribution/kernel-density-plot/
        /// </summary>
        /// <returns></returns>
        public override PlotModel Create() {
            var plotModel = new PlotModel() {
                TitleFontSize = 13,
                TitleFontWeight = FontWeights.Bold,
                IsLegendVisible = false,
            };

            var linearAxis = new LinearAxis() {
                Position = AxisPosition.Left,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
            };
            linearAxis.MajorGridlineStyle = LineStyle.Dash;
            linearAxis.MajorTickSize = 2;

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                Title = _title,
            };
            var uncertaintyLowerBound = 5;
            var uncertaintyUpperBound = 95;
            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;
            var maximumX = double.NegativeInfinity;
            var counter = 0;
            var numberofValuesRef = _data.First().Value.Count;
            OxyPalette palette = null;
            if (palette == null) {
                palette = CustomPalettes.DistinctTone(_data.Count);
            }
            var xKernel = new Dictionary<string, List<double>>();
            var yKernel = new Dictionary<string, List<double>>();
            using (var R = new RDotNetEngine()) {
                foreach (var item in _data) {
                    var x = new List<double>();
                    var y = new List<double>();
                    try {
                        R.SetSymbol("data", item.Value);
                        R.EvaluateNoReturn("kernel <- density(data, kernel = 'gaussian')");
                        yKernel[item.Key] = R.EvaluateNumericVector("kernel$x");
                        xKernel[item.Key] = R.EvaluateNumericVector("kernel$y");
                        maximumX = Math.Max(maximumX, xKernel[item.Key].Max());
                    } finally {
                    }
                }
            }

            foreach (var item in _data) {
                var numberofValues = item.Value.Count;
                var scaling = Math.Sqrt(numberofValues * 1d / numberofValuesRef);
                var areaSeries1 = new AreaSeries() {
                    Color = palette.Colors[counter],
                    MarkerType = MarkerType.None,
                    StrokeThickness = .5
                };
                var areaSeries2 = new AreaSeries() {
                    Color = palette.Colors[counter],
                    MarkerType = MarkerType.None,
                    StrokeThickness = .5
                };
                for (int i = 0; i < xKernel[item.Key].Count(); i++) {
                    areaSeries1.Points.Add(new DataPoint(xKernel[item.Key][i] * scaling + counter, yKernel[item.Key][i]));
                    areaSeries2.Points.Add(new DataPoint(-xKernel[item.Key][i] * scaling + counter, yKernel[item.Key][i]));
                };
                var series = new BoxPlotSeries() {
                    Fill = OxyColor.FromAColor(100, palette.Colors[counter]),
                    StrokeThickness = .5,
                    Stroke = OxyColors.Black,
                    WhiskerWidth = .05,
                    BoxWidth = .05,
                };
                var dp = asBoxPlotDataPoint(item.Value, lowerBound: uncertaintyLowerBound, upperBound: uncertaintyUpperBound);
                categoryAxis.Labels.Add($"{counter}");
                var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median, dp.UpperBox, dp.UpperWisker) {
                    Outliers = dp.Outliers,
                    Mean = item.Value.Average(),
                };
                series.Items.Add(boxPlotItem);
                minimum = Math.Min(minimum, dp.LowerWisker);
                maximum = Math.Max(maximum, dp.UpperWisker);
                linearAxis.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
                linearAxis.MajorStep = linearAxis.MajorStep > 0 ? linearAxis.MajorStep : double.NaN;
                plotModel.Series.Add(series);
                plotModel.Series.Add(areaSeries1);
                plotModel.Series.Add(areaSeries2);
                counter++;
            }
            plotModel.Axes.Add(linearAxis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}
