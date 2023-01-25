using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MathNet.Numerics.Distributions;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Core.Drawing;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class BetaScaledDensityTests : OxyPlotLineCreator {

        private static void createToFile(PlotModel plotModel, string filename) {
            var thread = new Thread(() => {
                plotModel.Background = OxyColors.White;
                PngExporter.Export(plotModel, filename, 500, 350, 96);
            });
            thread.Start();
            thread.Join();
        }

        public override string ChartId {
            get {
                return null;
            }
        }

        public override PlotModel Create() {
            return null;
        }

        /// <summary>
        /// Using Numerics for scaled beta
        /// </summary>
        [TestMethod]
        public void BetaScaledNumericsTest2() {
            var a = 0.7D;
            var b = 0.5D;
            var c = 1d;
            var d = 2d;
            var betaScaled = new BetaScaled(a, b, c, d);
            var steps = 100;
            var n = 50;
            var pdf = new List<double>();
            var x_as = new List<double>();
            var delta = (d - c) / steps;
            var L = c;
            for (int i = 0; i < steps + n; i++) {
                var pdfPt = betaScaled.Density(L);
                if (!double.IsInfinity(pdfPt)) {
                    x_as.Add(L);
                    pdf.Add(pdfPt);
                }
                L += delta;
            }
            var plotModel = createDefaultPlotModel($"Exposure: Beta a={a}, b={b}, c={c}, d={d}");

            var lineSeries = createDefaultLineSeries();
            for (int i = 0; i < pdf.Count; i++) {

                lineSeries.Points.Add(new DataPoint(x_as[i], pdf[i]));
            }
            plotModel.Series.Add(lineSeries);

            var linearAxis = createDefaultBottomLinearAxis();
            linearAxis.Title = "x";
            linearAxis.Minimum = 0;
            linearAxis.Maximum = x_as.Max() * 1.05;
            plotModel.Axes.Add(linearAxis);

            var verticalAxis = new LinearAxis();
            verticalAxis.Title = "Probability";
            verticalAxis.Minimum = 0;
            verticalAxis.Maximum = pdf.Max() * 1.05;
            plotModel.Axes.Add(verticalAxis);
            plotModel.Background = OxyColors.White;

            createToFile(plotModel, TestResourceUtilities.ConcatWithOutputPath("BetaScaledDensity.png"));
        }
    }
}