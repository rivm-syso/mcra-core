using MCRA.General;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class DriverCompoundsChartCreatorBase : ReportChartCreatorBase {

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

        protected (PlotModel, List<(string Substance, string Target)> drivers, double[] exposures, int maximumNumberPalette) createMCRChart(
            List<DriverSubstanceRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            double? percentage,
            double threshold,
            string xTitle,
            bool skipPrivacySensitiveOutputs = false,
            bool renderLegend = true
         ) {
            if (percentiles.Length == 0) {
                percentiles = [50];
            }
            var minimumExposure = double.NaN;
            if (percentage != null) {
                minimumExposure = drivers
                    .Select(c => c.CumulativeExposure)
                    .Percentile((double)percentage) * 0.5;
            } else {
                minimumExposure = drivers.Min(c => c.CumulativeExposure);
            }

            var totalExposure = drivers.Sum(c => c.CumulativeExposure);
            var groups = drivers.GroupBy(c => c.SubstanceCode);
            var maximumNumberPalette = groups.Count();
            var selectedDrivers = groups
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
                .ToList();

            if (selectedDrivers.Count > 0 && minimumExposure > 0) {
                drivers = [.. drivers
                    .Where(c => selectedDrivers.Select(s => s.SubstanceCode).Contains(c.SubstanceCode)
                        && selectedDrivers.Select(s => s.Target).Contains(c.Target))];
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
                Minimum = percentage == null ? double.NaN : minimumExposure,
                Maximum = maximumExposure,
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratioMax,
            };
            plotModel.Axes.Add(linearAxis2);
            if (!double.IsNaN(threshold)) {
                var lineSeries = createLineSeries(OxyColors.Red);
                lineSeries.Points.Add(new DataPoint(threshold, 1));
                lineSeries.Points.Add(new DataPoint(threshold, ratioMax));
                plotModel.Series.Add(lineSeries);
            }

            if (!skipPrivacySensitiveOutputs) {
                var basePalette = OxyPalettes.Rainbow(maximumNumberPalette == 1 ? 2 : maximumNumberPalette);
                var counter = 0;

                foreach (var driver in selectedDrivers) {
                    var scatterSeries = new ScatterSeries() {
                        MarkerSize = 2,
                        MarkerType = MarkerType.Circle,
                        Title = $"{driver.SubstanceName}-{driver.Target}",
                        MarkerFill = basePalette.Colors.ElementAt(counter),
                        RenderInLegend = renderLegend,
                    };
                    var set = drivers
                        .Where(c => c.SubstanceCode == driver.SubstanceCode
                            && c.Target == driver.Target)
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
                    var minimum = !double.IsNaN(threshold) ? Math.Min(minimumExposure, threshold) * 0.9 : minimumExposure * 0.9;
                    lineSeries.Points.Add(new DataPoint(minimum, ratioCutOff));
                    lineSeries.Points.Add(new DataPoint(maximumExposure, ratioCutOff));
                    plotModel.Series.Add(lineSeries);
                }
            }
            return (plotModel, selectedDrivers.Select(c => (c.SubstanceCode, c.Target)).ToList(), percentilesExposure, maximumNumberPalette);
        }

        protected PlotModel createMCRChart(
            List<DriverSubstanceRecord> drivers,
            RiskMetricType riskMetricType,
            double threshold,
            string xTitle,
            bool skipPrivacySensitiveOutputs = false,
            bool renderLegend = true
        ) {
            drivers = [.. drivers.Where(c => !double.IsNaN(c.Ratio) && c.CumulativeExposure > 0)];
            var minimumExposure = drivers.Min(c => c.CumulativeExposure);
            var newThreshold = threshold;
            var totalExposure = drivers.Sum(c => c.CumulativeExposure);
            var groups = drivers.GroupBy(c => c.SubstanceCode);
            var maximumNumberPalette = groups.Count();
            var selectedDrivers = groups
                .Select(c => (
                    SubstanceCode: c.Key,
                    SubstanceName: c.First().SubstanceName,
                    Target: c.First().Target,
                    ExposureContribution: c.Sum(r => r.CumulativeExposure) / totalExposure * 100,
                    CumulativeExpoures: c.Where(c => c.CumulativeExposure > minimumExposure).Select(c => c.CumulativeExposure).ToList(),
                    N: c.Count()
                ))
                .Where(c => c.CumulativeExpoures.Any())
                .OrderByDescending(c => c.N)
                .ThenByDescending(c => c.SubstanceName)
                .ToList();

            if (selectedDrivers.Count > 0 && minimumExposure > 0) {
                drivers = [.. drivers
                    .Where(c => selectedDrivers.Select(s => s.SubstanceCode).Contains(c.SubstanceCode)
                        && selectedDrivers.Select(s => s.Target).Contains(c.Target))];
            }

            var cumulativeExposures = drivers
                .Select(c => c.CumulativeExposure)
                .ToList();
            var ratios = drivers
                .Select(c => c.Ratio)
                .ToList();

            minimumExposure = double.IsNaN(minimumExposure) ? cumulativeExposures.Min() : minimumExposure;
            var maximumExposure = Math.Max(cumulativeExposures.Max(), newThreshold);
            var ratioMax = ratios.Max() * 1.1;
            var ratioMin = ratios.Min();

            var plotModel = createPlotModel(string.Empty);
            var logarithmicAxis1 = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = xTitle,
                Minimum = minimumExposure * 0.9,
                Maximum = maximumExposure,
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratioMax,
            };
            plotModel.Axes.Add(linearAxis2);

            if (!skipPrivacySensitiveOutputs) {
                var basePalette = OxyPalettes.Rainbow(maximumNumberPalette == 1 ? 2 : maximumNumberPalette);
                var counter = 0;

                foreach (var driver in selectedDrivers) {
                    var scatterSeries = new ScatterSeries() {
                        MarkerSize = 2.5,
                        MarkerType = MarkerType.Circle,
                        Title = $"{driver.SubstanceName}-{driver.Target}",
                        MarkerFill = renderLegend ? basePalette.Colors.ElementAt(counter) : OxyColor.FromAColor(100, OxyColors.Black),
                        MarkerStroke = renderLegend ? basePalette.Colors.ElementAt(counter) : OxyColors.Black,
                        RenderInLegend = renderLegend,
                    };
                    var set = drivers
                        .Where(c => c.SubstanceCode == driver.SubstanceCode
                            && c.Target == driver.Target)
                        .ToList();
                    for (int i = 0; i < set.Count; i++) {
                        scatterSeries.Points.Add(new ScatterPoint(set[i].CumulativeExposure, set[i].Ratio));
                    }
                    plotModel.Series.Add(scatterSeries);
                    counter++;
                }

                var lineSeriesRatioTwo = createLineSeries(OxyColors.Red);
                var lineSeriesThreshold = createLineSeries(OxyColors.Red);
                var maxY = (int)Math.Ceiling(ratioMax);
                lineSeriesThreshold.Points.Add(new DataPoint(newThreshold, 1));
                lineSeriesThreshold.Points.Add(new DataPoint(newThreshold, maxY));
                plotModel.Series.Add(lineSeriesThreshold);
                lineSeriesRatioTwo.Points.Add(new DataPoint(minimumExposure * 0.9, 2));
                lineSeriesRatioTwo.Points.Add(new DataPoint(maximumExposure, 2));
                plotModel.Series.Add(lineSeriesRatioTwo);
                var lineSeriesCurve = createLineSeries(OxyColors.Red);

                var color = riskMetricType == RiskMetricType.ExposureHazardRatio ? OxyColors.CornflowerBlue : OxyColors.Red;
                var areaRegionColor = new AreaSeries() {
                    Color = OxyColor.FromAColor(75, color),
                    Fill = OxyColor.FromAColor(75, color),
                    StrokeThickness = 1,
                };
                var colorComplement = riskMetricType == RiskMetricType.ExposureHazardRatio ? OxyColors.Red : OxyColors.CornflowerBlue;
                var areaRegionColorComplement = new AreaSeries() {
                    Color = OxyColor.FromAColor(75, colorComplement),
                    Fill = OxyColor.FromAColor(75, colorComplement),
                    StrokeThickness = 1,
                };
                var point = newThreshold;

                if (riskMetricType == RiskMetricType.ExposureHazardRatio) {
                    var step = Math.Abs(maxY - newThreshold) / 20;
                    for (int i = 0; i < 21; i++) {
                        lineSeriesCurve.Points.Add(new DataPoint(point, point / newThreshold));
                        point += step;
                    }
                    plotModel.Series.Add(lineSeriesCurve);

                    areaRegionColor.Points.Add(new DataPoint(minimumExposure * 0.9, 1));
                    areaRegionColor.Points.Add(new DataPoint(minimumExposure * 0.9, maxY));
                    areaRegionColor.Points.Add(new DataPoint(newThreshold, maxY));
                    plotModel.Series.Add(areaRegionColor);
                    point = newThreshold;
                    for (int i = 0; i < 21; i++) {
                        areaRegionColorComplement.Points.Add(new DataPoint(point, point / newThreshold));
                        point += step;
                    }
                    areaRegionColorComplement.Points.Add(new DataPoint(maximumExposure, maxY));
                    plotModel.Series.Add(areaRegionColorComplement);
                    var regionRed = 100d * drivers.Count(c => c.CumulativeExposure >= newThreshold && c.CumulativeExposure >= newThreshold * c.Ratio) / drivers.Count;
                    var regionBlue = 100d * drivers.Count(c => c.CumulativeExposure < newThreshold) / drivers.Count;
                    var regionWhite = 100d - regionRed - regionBlue;
                    //shift annotation downwards
                    var delta = 0.05 * ratioMax;
                    if (regionRed > 0) {
                        var annotationRed = createLineAnnotation(ratioMax - delta, $"red: {regionRed:F1}%");
                        annotationRed.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationRed);
                    }
                    if (regionWhite > 0) {
                        var annotationWhite = createLineAnnotation(ratioMax - 2 * delta, $"white: {regionWhite:F1}%");
                        annotationWhite.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationWhite);
                    }
                    if (regionBlue > 0) {
                        var annotationBlue = createLineAnnotation(ratioMax - 3 * delta, $"blue: {regionBlue:F1}%");
                        annotationBlue.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationBlue);
                    }
                } else {
                    var step = Math.Abs(newThreshold - 1d / maxY) / 20;
                    for (int i = 0; i < 21; i++) {
                        lineSeriesCurve.Points.Add(new DataPoint(point, newThreshold / point));
                        point -= step;
                    }
                    plotModel.Series.Add(lineSeriesCurve);
                    areaRegionColorComplement.Points.Add(new DataPoint(newThreshold, 1));
                    areaRegionColorComplement.Points.Add(new DataPoint(newThreshold, maxY));
                    areaRegionColorComplement.Points.Add(new DataPoint(maximumExposure, maxY));
                    plotModel.Series.Add(areaRegionColorComplement);

                    areaRegionColor.Points.Add(new DataPoint(minimumExposure * 0.9, maxY));
                    point = 1d / maxY;
                    for (int i = 21; i > 0; i--) {
                        areaRegionColor.Points.Add(new DataPoint(point, newThreshold / point));
                        point += step;
                    }
                    plotModel.Series.Add(areaRegionColor);
                    var regionRed = 100d * drivers.Count(c => c.CumulativeExposure <= newThreshold && c.CumulativeExposure <= newThreshold / c.Ratio) / drivers.Count;
                    var regionBlue = 100d * drivers.Count(c => c.CumulativeExposure > newThreshold) / drivers.Count;
                    var regionWhite = 100d - regionRed - regionBlue;
                    //shift annotation downwards
                    var delta = 0.05 * ratioMax;
                    if (regionRed > 0) {
                        var annotationRed = createLineAnnotation(ratioMax - delta, $"red: {regionRed:F1}%");
                        annotationRed.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationRed);
                    }
                    if (regionWhite > 0) {
                        var annotationWhite = createLineAnnotation(ratioMax - 2 * delta, $"white: {regionWhite:F1}%");
                        annotationWhite.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationWhite);
                    }
                    if (regionBlue > 0) {
                        var annotationBlue = createLineAnnotation(ratioMax - 3 * delta, $"blue: {regionBlue:F1}%");
                        annotationBlue.MaximumX = maximumExposure;
                        plotModel.Annotations.Add(annotationBlue);
                    }
                }
            }
            return plotModel;
        }
    }
}
