using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MathNet.Numerics.Distributions;
using MCRA.General;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class DoseResponseDataChartCreator : OxyPlotLineCreator {

        private static MarkerType[] _markerTypes = new MarkerType[] { MarkerType.Circle, MarkerType.Diamond, MarkerType.Square, MarkerType.Triangle };

        protected DoseResponseExperimentSection _section;

        public DoseResponseDataChartCreator(DoseResponseExperimentSection section, int width, int height) {
            Width = width;
            Height = height; ;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "1D7D0189-7BC0-4E50-8049-1EDF1C57094D";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section);
        }

        public override string Title => $"Dose-response data {_section.ExperimentCode}-{_section.ResponseCode}";

        private PlotModel create(DoseResponseExperimentSection section) {
            var xtitle = $"dose ({section.DoseUnit})";
            var ytitle = string.IsNullOrEmpty(section.ResponseUnit) ? $"{section.ResponseCode}" : $"{section.ResponseCode} ({section.ResponseUnit})";
            return createPlotModel(section, xtitle, ytitle, false, double.NaN, out _, out _);
        }

        protected virtual PlotModel createPlotModel(DoseResponseExperimentSection section, string xtitle, string ytitle, bool rescaleRpf, double benchMarkDoseMinimum, out double doseMinimum, out double doseMaximum) {
            var plotModel = createDefaultPlotModel();
            if (section.DoseResponseSets.Count > 1) {
                setLegend(plotModel);
            }
            var basePalette = CustomPalettes.DistinctTone(section.DoseResponseSets.Count + 1);
            var doseResponseData = section.DoseResponseSets
                .SelectMany(c => c.DoseResponseRecords, (c, r) => (
                    dose: (rescaleRpf ? c.RPF : 1) * r.Dose,
                    response: r.Response
                ))
                .ToList();

            if (doseResponseData.Any(r => r.dose > 0)) {
                var doses = doseResponseData.Where(c => c.dose > 0);
                doseMinimum = doses.Min(c => c.dose) * 0.2;
                doseMaximum = doseResponseData.Max(c => c.dose) * 1.05;
            } else {
                doseMinimum = 0.001;
                doseMaximum = 1;
            }

            if (!double.IsNaN(benchMarkDoseMinimum)) {
                if (doseMinimum > benchMarkDoseMinimum) {
                    doseMinimum = benchMarkDoseMinimum * .2;
                }
            }

            var responseValues = doseResponseData.Where(c => c.response > 0).Select(r => r.response).ToList();
            var responseMinimum = responseValues.DefaultIfEmpty(0.1).Min();
            var responseMaximum = responseValues.DefaultIfEmpty(1).Max();

            var horizontalAxis = createLogarithmicAxis(xtitle);
            horizontalAxis.Minimum = doseMinimum;
            plotModel.Axes.Add(horizontalAxis);

            var enumInt = 0;
            foreach (var record in section.DoseResponseSets) {
                var indexSubstance = section.DoseResponseSets.Count > 1 && record.RPF == 1;
                plotDoseResponseData(plotModel, basePalette, record, enumInt, doseMinimum, rescaleRpf, indexSubstance);
                plotErrorBarsDoseResponseData(plotModel, record, section, doseMinimum, basePalette, enumInt, rescaleRpf, ref responseMinimum, ref responseMaximum);
                enumInt++;
            }

            if (section.IsMixture && rescaleRpf) {
                plotMixtureData(section, plotModel, basePalette, enumInt, doseMinimum);
                plotErrorBarsMixtureData(section, plotModel, basePalette, enumInt, doseMinimum, ref responseMinimum, ref responseMaximum);
                enumInt++;
            }

            if (section.ResponseType == ResponseType.Quantal || section.ResponseType == ResponseType.QuantalGroup) {
                var verticalAxis = createLinearAxis("Prob(" + ytitle + ")");
                verticalAxis.Position = AxisPosition.Left;
                verticalAxis.Minimum = -0.05;
                verticalAxis.Maximum = 1.05;
                plotModel.Axes.Add(verticalAxis);
            } else {
                var verticalAxis = createLogarithmicAxis(ytitle);
                verticalAxis.Position = AxisPosition.Left;
                verticalAxis.Minimum = responseMinimum * 0.95;
                verticalAxis.Maximum = responseMaximum * 1.05;
                plotModel.Axes.Add(verticalAxis);
            }

            return plotModel;
        }

        /// <summary>
        /// Plot data of dose response
        /// </summary>
        /// <param name="plotModel"></param>
        /// <param name="basePalette"></param>
        /// <param name="record"></param>
        /// <param name="enumInt"></param>
        /// <param name="doseMinimum"></param>
        private void plotDoseResponseData(PlotModel plotModel, OxyPalette basePalette, DoseResponseSet record, int enumInt, double doseMinimum, bool rescaleRpf, bool isIndexSubstance) {
            var substanceName = record.SubstanceName;
            //if (isIndexSubstance) {
            //    substanceName = "Index substance: " + substanceName;
            //}
            var series = createScatterSeries(basePalette, enumInt, $"{substanceName} {record.CovariateLevel}");
            foreach (var item in record.DoseResponseRecords) {
                series.Points.Add(new ScatterPoint(item.Dose == 0 ? doseMinimum : (rescaleRpf ? record.RPF : 1) * item.Dose, item.Response));
            }
            plotModel.Series.Add(series);
        }

        /// <summary>
        /// Error bars of dose response data
        /// Get mu and sigma of the log-normal distribution from a given mean and variance:
        ///    mu = Math.Log(Math.Pow(mean, 2) / Math.Sqrt(variance + Math.Pow(mean, 2)));
        ///    sigma = Math.Sqrt(Math.Log(1 + variance / (Math.Pow(mean, 2))));
        /// </summary>
        /// <param name="plotModel"></param>
        /// <param name="record"></param>
        /// <param name="section"></param>
        /// <param name="doseMinimum"></param>
        /// <param name="basePalette"></param>
        /// <param name="enumInt"></param>
        private static void plotErrorBarsDoseResponseData(PlotModel plotModel, DoseResponseSet record, DoseResponseExperimentSection section, double doseMinimum, OxyPalette basePalette, int enumInt, bool rescaleRpf, ref double responseMinimum, ref double responseMaximum) {
            var plotErrorBars = true;
            if (record.DoseResponseRecords.Any(r => r.N > 1 && !double.IsNaN(r.SD))) {
                var errorBars = record.DoseResponseRecords
                    .Select(c => {
                        var mean = c.Response;
                        var variance = Math.Pow(c.SD, 2);
                        var mu = Math.Log(Math.Pow(mean, 2) / Math.Sqrt(variance + Math.Pow(mean, 2)));
                        var sigma = Math.Sqrt(Math.Log(1 + variance / Math.Pow(mean, 2)));
                        var gm = Math.Exp(mu);
                        var gse = sigma / Math.Sqrt(c.N);
                        var df = c.N - 1;
                        var t = df >= 1 ? StudentT.InvCDF(0, 1, df, 0.975) : 0;
                        var tsd = df >= 1 ? gse * t : 0;
                        return (
                            dose: c.Dose == 0 ? doseMinimum : (rescaleRpf ? record.RPF : 1) * c.Dose,
                            errorY: gm,
                            errorYUpper: Math.Exp(tsd) * gm,
                            errorYLower: gm / Math.Exp(tsd)
                        );
                    })
                    .ToList();
                var seriesMean = createCustomScatterErrorSeries(basePalette, enumInt);
                foreach (var item in errorBars) {
                    if (!double.IsNaN(item.errorYLower) && !double.IsInfinity(item.errorYLower) && item.errorYLower < responseMinimum) {
                        responseMinimum = item.errorYLower;
                    }
                    if (!double.IsNaN(item.errorYUpper) && !double.IsInfinity(item.errorYUpper) && item.errorYUpper > responseMaximum) {
                        responseMaximum = item.errorYUpper;
                    }
                    seriesMean.Points.Add(new CustomScatterErrorPoint(
                        item.dose, double.NaN, double.NaN, item.errorY, item.errorYLower, item.errorYUpper, plotErrorBars)
                    );
                }
                plotModel.Series.Add(seriesMean);
            } else {
                var errorBars = record.DoseResponseRecords
                    .GroupBy(gr => gr.Dose)
                    .Select(c => {
                        double geometricMean, tsd;
                        getDoseGroupStatistics(c, out geometricMean, out tsd);
                        return (
                            dose: c.Key,
                            meanResponse: geometricMean,
                            errorYUpper: Math.Exp(tsd) * geometricMean,
                            errorYLower: geometricMean / Math.Exp(tsd)
                        );
                    })
                    .ToList();
                var seriesMean = createCustomScatterErrorSeries(basePalette, enumInt);
                foreach (var item in errorBars) {
                    if (!double.IsNaN(item.errorYLower) && !double.IsInfinity(item.errorYLower) && item.errorYLower < responseMinimum) {
                        responseMinimum = item.errorYLower;
                    }
                    if (!double.IsNaN(item.errorYUpper) && !double.IsInfinity(item.errorYUpper) && item.errorYUpper > responseMaximum) {
                        responseMaximum = item.errorYUpper;
                    }
                    var dose = item.dose == 0 ? doseMinimum : (rescaleRpf ? record.RPF : 1) * item.dose;
                    seriesMean.Points.Add(new CustomScatterErrorPoint(
                        dose, double.NaN, double.NaN, item.meanResponse, item.errorYLower, item.errorYUpper, plotErrorBars)
                    );
                }
                plotModel.Series.Add(seriesMean);
            }
        }

        /// <summary>
        /// Plot data of dose response mixture
        /// </summary>
        /// <param name="section"></param>
        /// <param name="plotModel"></param>
        /// <param name="basePalette"></param>
        /// <param name="enumInt"></param>
        /// <param name="doseMinimum"></param>
        private void plotMixtureData(DoseResponseExperimentSection section, PlotModel plotModel, OxyPalette basePalette, int enumInt, double doseMinimum) {
            var series = createScatterSeries(basePalette, enumInt, "mixture");
            foreach (var item in section.DoseResponseMixtureSet.CumulativeExposure()) {
                series.Points.Add(new ScatterPoint(item.Dose == 0 ? doseMinimum : item.Dose, item.Response));
            }
            plotModel.Series.Add(series);
        }

        /// <summary>
        /// Error bars of dose response mixture
        /// </summary>
        /// <param name="section"></param>
        /// <param name="plotModel"></param>
        /// <param name="basePalette"></param>
        /// <param name="enumInt"></param>
        /// <param name="doseMinimum"></param>
        private void plotErrorBarsMixtureData(DoseResponseExperimentSection section, PlotModel plotModel, OxyPalette basePalette, int enumInt, double doseMinimum, ref double responseMinimum, ref double responseMaximum) {
            var cumulativeExposures = section.DoseResponseMixtureSet.CumulativeExposure();
            var errorBars = cumulativeExposures
                .GroupBy(gr => gr.Dose)
                .Select(c => {
                    double geometricMean, tsd;
                    getDoseGroupStatistics(c, out geometricMean, out tsd);
                    return (
                        dose: c.Key,
                        meanResponse: geometricMean,
                        errorYUpper: Math.Exp(tsd) * geometricMean,
                        errorYLower: geometricMean / Math.Exp(tsd)
                    );
                })
                .ToList();
            var seriesMean = createCustomScatterErrorSeries(basePalette, enumInt);
            foreach (var item in errorBars) {
                if (!double.IsNaN(item.errorYLower) && !double.IsInfinity(item.errorYLower) && item.errorYLower < responseMinimum) {
                    responseMinimum = item.errorYLower;
                }
                if (!double.IsNaN(item.errorYUpper) && !double.IsInfinity(item.errorYUpper) && item.errorYUpper > responseMaximum) {
                    responseMaximum = item.errorYUpper;
                }
                var dose = item.dose == 0 ? doseMinimum : item.dose;
                seriesMean.Points.Add(new CustomScatterErrorPoint(
                    dose, double.NaN, double.NaN, item.meanResponse, item.errorYLower, item.errorYUpper, true)
                );
            }
            plotModel.Series.Add(seriesMean);
        }

        /// <summary>
        /// Get statistics needed for error bars
        /// </summary>
        /// <param name="c"></param>
        /// <param name="geometricMean"></param>
        /// <param name="tsd"></param>
        private static void getDoseGroupStatistics(IGrouping<double, DoseResponseRecord> c, out double geometricMean, out double tsd) {
            var df = c.Count() - 1;
            geometricMean = Math.Exp(c.Average(r => Math.Log(r.Response)));
            var se = Math.Sqrt(c.Select(r => Math.Log(r.Response)).ToList().Variance() / c.Count());
            var t = df >= 1 ? StudentT.InvCDF(0, 1, df, 0.975) : 0;
            tsd = df >= 1 ? se * t : 0;
        }

        private static ScatterSeries createScatterSeries(OxyPalette basePalette, int enumInt, string name) {
            return new ScatterSeries() {
                MarkerFill = basePalette.Colors.ElementAt(enumInt),
                MarkerSize = 4,
                MarkerType = _markerTypes[enumInt % _markerTypes.Length],
                Title = name,
                MarkerStroke = OxyColors.Black,
                MarkerStrokeThickness = 0.2,
            };
        }

        private static CustomScatterErrorSeries createCustomScatterErrorSeries(OxyPalette basePalette, int enumInt) {
            return new CustomScatterErrorSeries() {
                ErrorBarColor = basePalette.Colors.ElementAt(enumInt),
                MarkerStroke = OxyColors.Black,
                MarkerStrokeThickness = 0.2,
                MarkerSize = 4,
                MarkerType = MarkerType.None,
            };
        }

        protected void setLegend(PlotModel plotModel) {
            plotModel.LegendBackground = OxyColor.FromArgb(200, 255, 255, 255);
            plotModel.LegendBorder = OxyColors.Undefined;
            plotModel.LegendOrientation = LegendOrientation.Vertical;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.RightTop;
            plotModel.LegendFontSize = 13;
            plotModel.IsLegendVisible = true;
        }
    }
}
