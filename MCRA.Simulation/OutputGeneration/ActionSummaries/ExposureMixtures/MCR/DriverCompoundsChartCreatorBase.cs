using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class DriverCompoundsChartCreatorBase : OxyPlotChartCreator {

        protected PlotModel createPlotModel(string title) {
            var plotModel = new PlotModel() {
                Title = title,
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 12,
                IsLegendVisible = true,
                ClipTitle = false
            };

            var legend = new Legend {
                LegendBackground = OxyColor.FromArgb(200, 255, 255, 255),
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomLeft,
                LegendFontSize = 10,
            };
            plotModel.Legends.Add(legend);

            return plotModel;
        }

        protected LineSeries createLineSeries(OxyColor color) {
            return new LineSeries() {
                Color = color,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
            };
        }

        protected LineAnnotation createLineAnnotation(double y, string text) {
            return new LineAnnotation() {
                Type = LineAnnotationType.Horizontal,
                Color = OxyColors.Undefined,
                Y = y,
                Text = text,
                FontSize = 10,
            };
        }

        protected (PlotModel, List<(string Substance, string Target)> drivers, double[] exposures) createMCRChart(
            List<DriverSubstanceRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            double? percentage,
            string xTitle
         ) {
            if (percentiles.Length == 0) {
                percentiles = new double[1] { 50 };
            }
            var minimumExposure = double.NaN;
            if (percentage != null) {
                minimumExposure = drivers
                    .Select(c => c.CumulativeExposure)
                    .Percentile((double)percentage);
            } else {
                minimumExposure = drivers.Min(c => c.CumulativeExposure);
            }

            var totalExposure = drivers.Sum(c => c.CumulativeExposure);
            var selectedDrivers = drivers
                .GroupBy(c => c.SubstanceCode)
                .Select(c => (
                    SubstanceCode: c.Key,
                    SubstanceName: c.First().SubstanceName,
                    Target: c.First().Target,
                    ExposureContribution: c.Sum(r => r.CumulativeExposure) / totalExposure * 100,
                    CumulativeExpoures: c.Where(c => c.CumulativeExposure > minimumExposure).Select(c => c.CumulativeExposure).ToList(),
                    N: c.Count()
                ))
                .Where(c => c.ExposureContribution >= minimumPercentage && c.CumulativeExpoures.Any())
                .OrderByDescending(c => c.N)
                .ThenByDescending(c => c.SubstanceName)
                .Select(c => c)
                .ToList();

            if (selectedDrivers.Count > 0 && minimumExposure > 0) {
                drivers = drivers
                    .Where(c => selectedDrivers.Select(s => s.SubstanceCode).Contains(c.SubstanceCode)
                        && selectedDrivers.Select(s => s.Target).Contains(c.Target))
                    .ToList();
            }

            var cumulativeExposures = drivers
                .Select(c => c.CumulativeExposure)
                .ToList();
            var ratios = drivers
                .Select(c => c.Ratio)
                .ToList();

            var percentilesExposure = cumulativeExposures.Percentiles(percentiles);
            var pRatio = ratios.Percentiles(percentiles);
            minimumExposure = double.IsNaN(minimumExposure) ? cumulativeExposures.Min() : minimumExposure;
            var maximumExposure = cumulativeExposures.Max();
            var ratioMax = ratios.Max();
            var ratioMin = ratios.Min();

            var plotModel = createPlotModel(string.Empty);
            var logarithmicAxis1 = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = xTitle,
                Minimum = minimumExposure,
                Maximum = maximumExposure,
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratioMax,
            };
            plotModel.Axes.Add(linearAxis2);

            var basePalette = OxyPalettes.Rainbow(selectedDrivers.Count == 1 ? 2 : selectedDrivers.Count);
            var counter = 0;

            foreach (var driver in selectedDrivers) {
                var scatterSeries = new ScatterSeries() {
                    MarkerSize = 2,
                    MarkerType = MarkerType.Circle,
                    Title = $"{driver.SubstanceName}-{driver.Target}",
                    MarkerFill = basePalette.Colors.ElementAt(counter),
                };
                var set = drivers
                    .Where(c => c.SubstanceCode == driver.SubstanceCode
                        && c.Target == driver.Target)
                    .Select(c => c)
                    .ToList();
                for (int i = 0; i < set.Count; i++) {
                    scatterSeries.Points.Add(new ScatterPoint(set[i].CumulativeExposure, set[i].Ratio));
                }
                plotModel.Series.Add(scatterSeries);
                counter++;
            }

            var tUp = new List<string>();
            foreach (var item in percentiles) {
                tUp.Add($"p{item}");
            }
            for (int i = 0; i < percentilesExposure.Length; i++) {
                var lineSeries = createLineSeries(OxyColors.Black);
                lineSeries.Points.Add(new DataPoint(percentilesExposure[i], ratioMin));
                lineSeries.Points.Add(new DataPoint(percentilesExposure[i], ratioMax));
                plotModel.Series.Add(lineSeries);

                var lineAnnotation = createLineAnnotation(ratioMax * .95, tUp[i]);
                lineAnnotation.MaximumX = percentilesExposure[i] == logarithmicAxis1.Minimum ? percentilesExposure[i] * 1.1 : percentilesExposure[i];
                plotModel.Annotations.Add(lineAnnotation);
            }

            if (ratioCutOff > 0 && totalExposureCutOff == 0) {
                var lineSeries = createLineSeries(OxyColors.Gray);
                lineSeries.Points.Add(new DataPoint(minimumExposure, ratioCutOff));
                lineSeries.Points.Add(new DataPoint(maximumExposure, ratioCutOff));
                plotModel.Series.Add(lineSeries);
            }
            return (plotModel, selectedDrivers.Select(c => (c.SubstanceCode, c.Target)).ToList(), percentilesExposure);
        }
    }
}
