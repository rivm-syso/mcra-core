using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Core.Drawing;
using OxyPlot.Series;
using System;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests {

    /// <summary>
    /// TODO: refactor this class. It should not inherit from OxyPlotHeatMapCreator.
    /// </summary>
    [TestClass]
    public class HorizontalHeatMapSeriesTests : OxyPlotHeatMapCreator {

        public override string ChartId {
            get {
                return null;
            }
        }

        public override PlotModel Create() {
            return null;
        }

        [TestMethod]
        public void HeatMapSeriesTests_LinearHorizontal1() {
            var heatmapCreator = new HorizontalHeatmapCreator("LinearHorizontal");
            var plotModel = heatmapCreator.Create();
            var xLow = -10;
            var xHigh = 10;
            var yLow = -10;
            var yHigh = 10;
            var xNeutral = -3;
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh,
                YLow = yLow,
                YHigh = yHigh,
                HeatMapMappingFunction = (x, y) => {
                    var desirabilityX = 0D;
                    if (x >= xLow && x <= xNeutral) {
                        desirabilityX = 0.5 * (x - xLow) / (xNeutral - xLow);
                    } else if (x > xNeutral && x <= xHigh) {
                        desirabilityX = 0.5 + 0.5 * (x - xNeutral) / (xHigh - xNeutral);
                    } else if (x > xHigh) {
                        desirabilityX = 1;
                    }
                    return 2 * desirabilityX - 1;
                },
            };

            plotModel.Series.Add(heatMapSeries);

            var horizontalAxis = createHorizontalLinearAxis(xLow, xHigh);
            plotModel.Axes.Add(horizontalAxis);
            var verticalAxis = createVerticalLinearAxis(yLow, yHigh);
            plotModel.Axes.Add(verticalAxis);
            var lineSeries = new LineSeries() { MarkerType = MarkerType.Circle, MarkerSize = 20 };
            lineSeries.Points.Add(new DataPoint(3, 3));
            plotModel.Series.Add(lineSeries);

            heatmapCreator.CreateToFile(plotModel, TestResourceUtilities.ConcatWithOutputPath("HeatMapSeriesTests_LinearHorizontal1.png"));
        }

        [TestMethod]
        public void HeatMapSeriesTests_LinearDiagonal1() {
            var heatmapCreator = new HorizontalHeatmapCreator("LinearDiagonal");
            var plotModel = heatmapCreator.Create();
            var number = 1000;
            var xData = NormalDistribution.NormalSamples(number, 3.5, .5).ToList();
            var xPercentiles = xData.Percentiles(5, 95);
            var xRange = xPercentiles[1] - xPercentiles[0];
            var yData = NormalDistribution.NormalSamples(number, -1, 1).ToList();
            var yPercentiles = yData.Percentiles(5, 95);
            var yRange = yPercentiles[1] - yPercentiles[0];
            xData.Add(-5);
            yData.Add(-5);
            xData.Add(0);
            yData.Add(0);
            xData.Add(5);
            yData.Add(5);

            var xLow = -10;
            var xHigh = 10;
            var yLow = -10;
            var yHigh = 10;
            var steepness = 1d;
            var threshold = 7.389;
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh,
                YLow = yLow,
                YHigh = yHigh,
                HeatMapMappingFunction = (x, y) => {
                    var z = - y + x + Math.Log(threshold);
                    var desirabilityX = 1 / (1 + Math.Exp(-steepness * (z)));
                    var desirability = 2 * desirabilityX - 1;
                    return -desirability;
                },
            };
            plotModel.Title = "Risk21";
            plotModel.Series.Add(heatMapSeries);


            var horizontalAxis = createHorizontalLinearAxis(xLow, xHigh);
            horizontalAxis.Title = string.Format("logExposure (mg/kg bw/day)");
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createVerticalLinearAxis(yLow, yHigh);
            verticalAxis.StartPosition = 1;
            verticalAxis.EndPosition = 0;
            verticalAxis.Title = string.Format("logCED (mg/kg)");

            plotModel.Axes.Add(verticalAxis);

            var scatterSeries = new ScatterSeries();
            scatterSeries.MarkerSize = 2;
            scatterSeries.MarkerType = MarkerType.Circle;
            scatterSeries.MarkerFill = OxyColors.Black;
            for (int i = 0; i < xData.Count; i++) {
                scatterSeries.Points.Add(new ScatterPoint(xData[i], yData[i]));
            }
            plotModel.Series.Add(scatterSeries);


            var basePalette = OxyPalettes.Rainbow(2);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));

            var ellipseAnnotation = new EllipseAnnotation();
            ellipseAnnotation.X = xData.Average();
            ellipseAnnotation.Y = yData.Average();
            ellipseAnnotation.Width = xRange;
            ellipseAnnotation.Height = yRange;
            ellipseAnnotation.Fill = palette.ElementAt(1);
            ellipseAnnotation.StrokeThickness = 1;
            plotModel.Annotations.Add(ellipseAnnotation);

            heatmapCreator.CreateToFile(plotModel, TestResourceUtilities.ConcatWithOutputPath("HeatMapSeriesTests_LinearDiagonal1.png"));
        }


        [TestMethod]
        public void HeatMapSeriesTests_LogarithmicHorizontal1() {
            var heatmapCreator = new HorizontalHeatmapCreator("LogarithmicHorizontal");
            var plotModel = heatmapCreator.Create();
            var xLow = 1e-5;
            var xHigh = 1e5;
            var yLow = 100;
            var yHigh = 1e4;
            var xNeutral = 1e2;
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh,
                HeatMapMappingFunction = (x, y) => {
                    Func<double, double> transform = (val) => Math.Log10(val);
                    var logxNeutral = transform(xNeutral);
                    var logX = transform(x);
                    var desirabilityX = 0D;
                    if (logX <= logxNeutral) {
                        desirabilityX = 0.5 * (logX - transform(xLow)) / (logxNeutral - transform(xLow));
                    } else {
                        desirabilityX = 0.5 + 0.5 * (logX - logxNeutral) / (transform(xHigh) - logxNeutral);
                    }
                    return 2 * desirabilityX - 1;
                },
                YLow = yLow,
                YHigh = yHigh,
                ResolutionX = 100,
                ResolutionY = 100,
            };

            plotModel.Series.Add(heatMapSeries);

            var horizontalAxis = createHorizontalLogarithmicAxis(xLow, xHigh);
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createVerticalLogarithmicAxis(yLow, yHigh);
            plotModel.Axes.Add(verticalAxis);

            heatmapCreator.CreateToFile(plotModel, TestResourceUtilities.ConcatWithOutputPath("HeatMapSeriesTests_LogarithmicHorizontal1.png"));
        }

        [TestMethod]
        public void HeatMapSeriesTests_LogarithmicDiagonal1() {
            var heatmapCreator = new HorizontalHeatmapCreator("LogarithmicDiagonal");
            var plotModel = heatmapCreator.Create();
            var number = 1000;
            var xData = NormalDistribution.NormalSamples(number, 1, 0.2).Select(c => Math.Pow(10, c)).ToList();
            var xPercentiles = xData.Percentiles(5, 95);
            var xRange = xPercentiles[1] - xPercentiles[0];
            var yData = NormalDistribution.NormalSamples(number, -0.1, 0.1).Select(c => Math.Pow(10, c)).ToList();
            var yPercentiles = yData.Percentiles(5, 95);
            var yRange = yPercentiles[1] - yPercentiles[0];

            var xLow = xData.Min() * 0.001; 
            var xHigh = xData.Max() * 1000;
            var yLow = yData.Min() * 0.001;
            var yHigh = yData.Max() * 1000;
            var threshold = 1;

            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh,
                YLow = yLow,
                YHigh = yHigh,
                HeatMapMappingFunction = (x, y) => {
                    var z = y / (x * threshold);
                    var desirabilityX = z / (1 + z);
                    var desirability = 2 * desirabilityX - 1;
                    return desirability;
                },
            };
            plotModel.Series.Add(heatMapSeries);

            var horizontalAxis = createHorizontalLogarithmicAxis(xLow, xHigh);
            horizontalAxis.Title = string.Format("Exposure (mg/kg bw/day)");
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createVerticalLogarithmicAxis(yLow, yHigh);
            verticalAxis.StartPosition = 1;
            verticalAxis.EndPosition = 0;
            verticalAxis.Title = string.Format("CED (mg/kg)");
            plotModel.Axes.Add(verticalAxis);

            var scatterSeries = new ScatterSeries();
            scatterSeries.MarkerSize = 2;
            scatterSeries.MarkerType = MarkerType.Circle;
            scatterSeries.MarkerFill = OxyColors.Black;
            for (int i = 0; i < xData.Count; i++) {
                scatterSeries.Points.Add(new ScatterPoint(xData[i], yData[i]));
            }
            plotModel.Series.Add(scatterSeries);


            var basePalette = OxyPalettes.Rainbow(2);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));

            var ellipseAnnotation = new EllipseAnnotation();
            ellipseAnnotation.X = xData.Average();
            ellipseAnnotation.Y = yData.Average();
            ellipseAnnotation.Width = xRange;
            ellipseAnnotation.Height = yRange;
            ellipseAnnotation.Fill = palette.ElementAt(1);
            ellipseAnnotation.StrokeThickness = 1;
            plotModel.Annotations.Add(ellipseAnnotation);

            heatmapCreator.CreateToFile(plotModel, TestResourceUtilities.ConcatWithOutputPath("HeatMapSeriesTests_LogarithmicDiagonal1.png"));
        }
    }
}
