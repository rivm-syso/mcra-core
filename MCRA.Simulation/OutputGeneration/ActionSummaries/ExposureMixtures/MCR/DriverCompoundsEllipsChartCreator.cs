using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverCompoundsEllipsChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;

        public DriverCompoundsEllipsChartCreator(MaximumCumulativeRatioSection section) {
            Height = 400;
            Width = 500;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "1fd1a2c5-50df-4e5a-b3e8-4d59d1ff6b42";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Using MCR to identify substances that drive cumulative exposures, bivariate distributions.";

        public override PlotModel Create() {
            return create(_section.DriverCompoundStatisticsRecords, _section.DriverCompounds, _section.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix));
        }

        private PlotModel create(List<DriverCompoundStatisticsRecord> statistics, List<DriverCompoundRecord> drivers, string intakeUnit) {
            var cumulativeExposure = drivers.Select(c => c.CumulativeExposure).ToList();
            var pUp = cumulativeExposure.Percentiles(5, 50, 95);
            var ratio = drivers.Select(c => c.Ratio).ToList();
            var pRatio = ratio.Percentiles(5, 50, 95);

            var edChiSq = 2d;
            var plotModel = base.createPlotModel(string.Empty);

            var logarithmicAxis1 = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = $"Cumulative exposure ({intakeUnit})",
                Minimum = cumulativeExposure.Min(),
                Maximum = cumulativeExposure.Max(),
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratio.Max(),
            };
            plotModel.Axes.Add(linearAxis2);

            var maxN = statistics.Max(c => c.Number);
            var basePalette = OxyPalettes.Rainbow(statistics.Count == 1 ? 2 : statistics.Count);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));
            var counter = 0;
            foreach (var bivariate in statistics) {
                var xamp = Math.Sqrt(edChiSq) * bivariate.CVCumulativeExposure;
                var yamp = Math.Sqrt(edChiSq) * bivariate.CVRatio;
                var logTotalExposureMedian = Math.Log(bivariate.CumulativeExposureMedian);
                var logRatioMedian = Math.Log(bivariate.RatioMedian);
                var acosR = Math.Acos(bivariate.R);
                var x = new List<double>();
                var y = new List<double>();
                for (int i = 0; i < 362; i++) {
                    var t = i * (2 * Math.PI) / 360;
                    var _x = logTotalExposureMedian + xamp * Math.Cos(t);
                    var _y = logRatioMedian + yamp * Math.Cos(t + acosR);
                    x.Add(Math.Exp(_x));
                    y.Add(Math.Exp(_y));
                }

                var areaSeries = new AreaSeries() {
                    Title = bivariate.CompoundName,
                    Color = basePalette.Colors.ElementAt(counter),
                    Fill = palette.ElementAt(counter),
                    MarkerType = MarkerType.None,
                    StrokeThickness = 1,
                };

                for (int i = 0; i < x.Count; i++) {
                    areaSeries.Points.Add(new DataPoint(x[i], y[i]));
                }
                plotModel.Series.Add(areaSeries);
                counter++;
            }

            var tUp = new string[] { "p5", "p50", "p95" };
            for (int i = 0; i < pUp.Length; i++) {
                var lineSeries = createLineSeries(OxyColors.Black);
                lineSeries.Points.Add(new DataPoint(pUp[i], 1));
                lineSeries.Points.Add(new DataPoint(pUp[i], ratio.Max()));
                plotModel.Series.Add(lineSeries);

                var lineAnnotation = createLineAnnotation(ratio.Max() * .95, tUp[i]);
                lineAnnotation.MaximumX = pUp[i] == logarithmicAxis1.Minimum ? pUp[i] * 1.1 : pUp[i];
                plotModel.Annotations.Add(lineAnnotation);
            }

            for (int i = 0; i < pRatio.Length; i++) {
                var lineSeries = createLineSeries(OxyColors.Black);
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Max(), pRatio[i]));
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Min(), pRatio[i]));
                plotModel.Series.Add(lineSeries);

                var lineAnnotation = createLineAnnotation(pRatio[i], tUp[i]);
                plotModel.Annotations.Add(lineAnnotation);
            }
            return plotModel;
        }
    }
}
