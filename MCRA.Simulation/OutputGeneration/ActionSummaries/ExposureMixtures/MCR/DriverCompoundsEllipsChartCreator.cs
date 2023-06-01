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

        //protected PlotModel createNew(List<DriverCompoundStatisticsRecord> statistics, List<DriverCompoundRecord> drivers, string intakeUnit) {
        //    var cumulativeExposures = drivers.Select(c => c.CumulativeExposure).ToList();
        //    var pUp = cumulativeExposures.Percentiles(5, 50, 95);
        //    var ratios = drivers.Select(c => c.Ratio).ToList();
        //    var pRatio = ratios.Percentiles(5, 50, 95);

        //    var edChiSq = 2.4477;
        //    var plotModel = base.createPlotModel(String.Format("Bivariate distributions"));

        //    var logarithmicAxis1 = new LogarithmicAxis() {
        //        //var logarithmicAxis1 = new LinearAxis() {
        //        Position = AxisPosition.Bottom,
        //        Title = string.Format("Cumulative exposure ({0})", intakeUnit),
        //        Minimum = cumulativeExposures.Min(),
        //    };
        //    plotModel.Axes.Add(logarithmicAxis1);

        //    var linearAxis2 = new LinearAxis() {
        //        Title = "Maximum Cumulative Ratio",
        //        Minimum = 1,
        //    };
        //    plotModel.Axes.Add(linearAxis2);

        //    var maxN = statistics.Max(c => c.Number);
        //    var basePalette = OxyPalettes.Rainbow(statistics.Count == 1 ? 2 : statistics.Count);
        //    var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));
        //    var counter = 0;
        //    var xMaximum = 0d;
        //    var yMaximum = 0d;
        //    foreach (var bivariate in statistics) {
        //        var vcov = new GeneralMatrix(2, 2);

        //        var sdE = Math.Sqrt(Math.Log(Math.Pow(bivariate.CVCumulativeExposure, 2) + 1));
        //        var sdR = Math.Sqrt(Math.Log(Math.Pow(bivariate.CVRatio, 2) + 1));
        //        //var sdE = Math.Sqrt(bivariate.CVCumulativeExposure);
        //        //var sdR = Math.Sqrt(bivariate.CVRatio);
        //        vcov.SetElement(0, 0, sdE * sdE);
        //        vcov.SetElement(0, 1, bivariate.R * sdE * sdR);
        //        vcov.SetElement(1, 0, bivariate.R * sdE * sdR);
        //        vcov.SetElement(1, 1, sdR * sdR);
        //        var first = false;
        //        if (sdE > sdR) {
        //            first = true;
        //        }

        //        var eigenVector = vcov.Eigen();
        //        var largestEigenValue = eigenVector.RealEigenvalues.Max();
        //        var smallestEigenValue = eigenVector.RealEigenvalues.Min();
        //        var largestEigenVector = new double[2];
        //        if (first) {
        //            largestEigenVector = eigenVector.GetV().Array[0];
        //        } else {
        //            largestEigenVector = eigenVector.GetV().Array[1];
        //        }
        //        var angle = Math.Atan2(largestEigenVector[1], largestEigenVector[0]);
        //        if (angle < 0) {
        //            angle += 2 * Math.PI;
        //        }
        //        var a = edChiSq * Math.Sqrt(largestEigenValue);
        //        var b = edChiSq * Math.Sqrt(smallestEigenValue);
        //        var x = new List<double>();
        //        var y = new List<double>();
        //        var cosphi = Math.Cos(angle);
        //        var sinphi = Math.Sin(angle);

        //        var areaSeries = new AreaSeries() {
        //            Title = bivariate.CompoundName,
        //            Color = basePalette.Colors.ElementAt(counter),
        //            Fill = palette.ElementAt(counter),
        //            MarkerType = MarkerType.None,
        //            StrokeThickness = 1,
        //        };

        //        for (int i = 0; i < 360; i++) {
        //            var grid = i * (2 * Math.PI) / 359;
        //            var _x = a * Math.Cos(grid);
        //            var _y = b * Math.Sin(grid);
        //            x.Add(Math.Exp(_x * cosphi - _y * sinphi + Math.Log(bivariate.CumulativeExposureMedian)));
        //            //y.Add(Math.Exp(_x * sinphi + _y * cosphi + Math.Log(bivariate.RatioMedian)));
        //            //x.Add((_x * cosphi - _y * sinphi + (bivariate.CumulativeExposureMedian)));
        //            y.Add((_x * sinphi + _y * cosphi + (bivariate.RatioMedian)));
        //            areaSeries.Points.Add(new DataPoint(x[i], y[i]));
        //            if (x[i] > xMaximum) {
        //                xMaximum = x[i];
        //            }
        //            if (y[i] > yMaximum) {
        //                yMaximum = y[i];
        //            }
        //        }
        //        plotModel.Series.Add(areaSeries);

        //        var scatterSeries = new ScatterSeries() {
        //            MarkerSize = 4,
        //            MarkerType = MarkerType.Circle,
        //            //Title = bivariate.CompoundName,
        //            MarkerFill = basePalette.Colors.ElementAt(counter),
        //        };
        //        var set = drivers.Where(c => c.CompoundName == bivariate.CompoundName).Select(c => c).ToList();
        //        for (int i = 0; i < set.Count; i++) {
        //            scatterSeries.Points.Add(new ScatterPoint(set[i].CumulativeExposure, set[i].Ratio));
        //        }
        //        plotModel.Series.Add(scatterSeries);
        //        counter++;

        //    }
        //    logarithmicAxis1.Maximum = xMaximum * 1.05;
        //    linearAxis2.Maximum = yMaximum * 1.05;
        //    var tUp = new string[] { "p5", "p50", "p95" };
        //    for (int i = 0; i < pUp.Length; i++) {
        //        var lineSeries = createLineSeries(OxyColors.Black);
        //        lineSeries.Points.Add(new DataPoint(pUp[i], 1));
        //        lineSeries.Points.Add(new DataPoint(pUp[i], ratios.Max()));
        //        plotModel.Series.Add(lineSeries);

        //        var lineAnnotation = createLineAnnotation(ratios.Max() * .95, tUp[i]);
        //        lineAnnotation.MaximumX = pUp[i] == logarithmicAxis1.Minimum ? pUp[i] * 1.1 : pUp[i];
        //        plotModel.Annotations.Add(lineAnnotation);
        //    }

        //    for (int i = 0; i < pRatio.Length; i++) {
        //        var lineSeries = createLineSeries(OxyColors.Black);
        //        lineSeries.Points.Add(new DataPoint(cumulativeExposures.Max(), pRatio[i]));
        //        lineSeries.Points.Add(new DataPoint(cumulativeExposures.Min(), pRatio[i]));
        //        plotModel.Series.Add(lineSeries);

        //        var lineAnnotation = createLineAnnotation(pRatio[i], tUp[i]);
        //        plotModel.Annotations.Add(lineAnnotation);
        //    }
        //    return plotModel;

        //}

    }
}
