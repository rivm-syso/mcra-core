using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.DoseResponseModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class DoseResponseFitChartCreator : DoseResponseDataChartCreator {

        private bool _rescaleRpf;

        public DoseResponseFitChartCreator(DoseResponseModelSection section, int width, int height, bool rescaleRpf)
            : base(section, width, height) {
            _rescaleRpf = rescaleRpf;
        }

        public override string ChartId {
            get {
                var pictureId = "CDF6E82B-CCB1-482E-9417-FB7FBA8376CC";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _rescaleRpf);
            }
        }

        public override PlotModel Create() {
            return create(_section as DoseResponseModelSection);
        }

        public override string Title {
            get {
                if (_rescaleRpf) {
                    return $"Dose-response model fit {_section.ExperimentCode}";
                } else {
                    return $"Dose-response model fit {_section.ExperimentCode} (RPF scaled).";
                }
            }
        }

        private PlotModel create(DoseResponseModelSection section) {
            return createPlotModel(section);
        }

        protected PlotModel createPlotModel(DoseResponseModelSection section) {
            var rescaleRpf = section.DoseResponseFits.SelectMany(r => r.CovariateLevel).Distinct().Count() <= 1
                && section.DoseResponseFits.SelectMany(r => r.SubstanceCode).Distinct().Count() > 1;
            rescaleRpf = _rescaleRpf;
            var ytitle = string.IsNullOrEmpty(section.ResponseUnit) ? $"{section.ResponseCode}" : $"{section.ResponseCode} ({section.ResponseUnit})";
            var xtitle = $"dose ({section.DoseUnit})";
            if (rescaleRpf) {
                xtitle = $"dose ({section.DoseUnit} equivalents)";
            }

            var benchMarkDoseMinimum = double.NaN;
            if (section.DoseResponseFits != null && section.DoseResponseFits.Any()) {
                benchMarkDoseMinimum = section.DoseResponseFits.Min(r => (rescaleRpf ? r.RelativePotencyFactor : 1D) * r.BenchmarkDose);
            }

            var plotModel = base.createPlotModel(section, xtitle, ytitle, rescaleRpf, benchMarkDoseMinimum, out double doseMinimum, out double doseMaximum);
            if (section.DoseResponseFits != null && section.DoseResponseFits.Any()) {
                var doseValues = section.DoseResponseSets
                    .SelectMany(r => r.DoseResponseRecords.Select(drr => (rescaleRpf ? r.RPF : 1D) * drr.Dose));
                var responseValues = section.DoseResponseSets
                    .SelectMany(r => r.DoseResponseRecords.Select(drr => drr.Response));
                var responseMinimum = plotModel.Axes.First(r => r.Position == AxisPosition.Left).Minimum;
                doseMaximum = updateDoseMaximum(section, doseMaximum, rescaleRpf);
                plotFit(section, plotModel, doseMinimum, doseMaximum, rescaleRpf);
                plotCED(section, plotModel, doseMinimum * .1, responseMinimum, rescaleRpf);
            }
            return plotModel;
        }

        /// <summary>
        /// Updates the dose maximum (for the cases that the CEDs are greater than the largest dose).
        /// </summary>
        /// <param name="section"></param>
        /// <param name="doseMaximum"></param>
        /// <param name="rescaleRpf"></param>
        private double updateDoseMaximum(DoseResponseModelSection section, double doseMaximum, bool rescaleRpf) {
            var fitRecords = rescaleRpf ? section.DoseResponseFits.Where(r => r.RelativePotencyFactor == 1D) : section.DoseResponseFits;
            foreach (var fit in fitRecords) {
                var rpf = rescaleRpf ? fit.RelativePotencyFactor : 1D;
                var x = (fit.BenchmarkDose / rpf) * 1.05;
                if (x > doseMaximum) {
                    doseMaximum = x;
                }
            }
            return doseMaximum;
        }

        /// <summary>
        /// Line fit of dose response curve
        /// </summary>
        /// <param name="section"></param>
        /// <param name="plotModel"></param>
        /// <param name="doseMinimum"></param>
        private void plotFit(DoseResponseModelSection section, PlotModel plotModel, double doseMinimum, double doseMaximum, bool rescaleRpf) {
            var fitRecords = rescaleRpf ? section.DoseResponseFits.Where(r => r.RelativePotencyFactor == 1D) : section.DoseResponseFits;
            fitRecords = section.DoseResponseFits;
            foreach (var fit in fitRecords) {
                var modelType = DoseResponseModelTypeConverter.FromString(section.ModelType);
                var parameters = fit.ModelParameterValues
                    .Split(',').Select(c => c.Split('='))
                    .Where(r => r.Any())
                    .ToDictionary(x => x[0], x => Convert.ToDouble(x[1], CultureInfo.InvariantCulture));
                var model = DoseResponseModelFactory.Create(modelType, parameters);
                var doses = createDoseRange(doseMinimum, doseMaximum);
                var rpf = rescaleRpf ? fit.RelativePotencyFactor : 1D;
                var points = doses.Select(r => new DataPoint(r, model.Calculate(r / rpf)));
                var lineSeries = createBrokenLineSeries(OxyColors.Black);
                lineSeries.Points.AddRange(points);
                plotModel.Series.Add(lineSeries);
            }
        }

        /// <summary>
        /// Plot CED and Response
        /// </summary>
        /// <param name="section"></param>
        /// <param name="plotModel"></param>
        /// <param name="doseMinimum"></param>
        /// <param name="responseMinimum"></param>
        private void plotCED(DoseResponseModelSection section, PlotModel plotModel, double doseMinimum, double responseMinimum, bool rescaleRpf) {
            var fitRecords = rescaleRpf ? section.DoseResponseFits.Where(r => r.RelativePotencyFactor == 1D) : section.DoseResponseFits;
            foreach (var fit in fitRecords) {
                var rpf = rescaleRpf ? fit.RelativePotencyFactor : 1D;

                var lineSeriesVertical = createBrokenLineSeries(OxyColors.Black);
                lineSeriesVertical.Points.Add(new DataPoint(fit.BenchmarkDose / rpf, fit.BenchmarkResponse));
                lineSeriesVertical.Points.Add(new DataPoint(fit.BenchmarkDose / rpf, responseMinimum));
                plotModel.Series.Add(lineSeriesVertical);

                var lineSeriesHorizontal = createBrokenLineSeries(OxyColors.Black);
                lineSeriesHorizontal.Points.Add(new DataPoint(doseMinimum, fit.BenchmarkResponse));
                lineSeriesHorizontal.Points.Add(new DataPoint(fit.BenchmarkDose / rpf, fit.BenchmarkResponse));
                plotModel.Series.Add(lineSeriesHorizontal);
            }
        }

        private List<double> createDoseRange(double doseMinimum, double doseMaximum) {
            var xVar = GriddingFunctions.LogSpace(doseMinimum, doseMaximum, 100).ToList();
            return xVar;
        }

        private LineSeries createBrokenLineSeries(OxyColor color) {
            return new LineSeries() {
                Color = color,
                BrokenLineStyle = LineStyle.Dot,
                StrokeThickness = .75,
            };
        }
    }
}
