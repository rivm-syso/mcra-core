using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureHistogramChartCreatorBase : HistogramChartCreatorBase {

        protected PlotModel createPlotModel(
            List<HistogramBin> binsTransformed,
            List<HistogramBin> binsTransformedCoExposure,
            string title,
            string xtitle
        ) {
            var plotModel = createPlotModel(
                binsTransformed,
                title,
                xtitle
            );

            if (binsTransformedCoExposure != null && binsTransformedCoExposure.Count > 0) {
                plotModel.Series.First().Title = "Exposure";
                var bins = binsTransformedCoExposure.Select(r => new HistogramBin() {
                    Frequency = r.Frequency,
                    XMinValue = Math.Pow(10, r.XMinValue),
                    XMaxValue = Math.Pow(10, r.XMaxValue),
                }).ToList();

                var histogramSeries = createDefaultHistogramSeries(bins);
                histogramSeries.FillColor = OxyColors.Red;
                histogramSeries.Title = "Co-exposure";
                plotModel.Series.Add(histogramSeries);
            }
            return plotModel;
        }

        protected PlotModel create(
            List<HistogramBin> binsTransformed,
            List<HistogramBin> binsTransformedCoExpsoure,
            string title,
            string intakeUnit
        ) {
            var xAxisTitle = $"Exposure ({intakeUnit})";
            var plotModel = createPlotModel(
                binsTransformed,
                binsTransformedCoExpsoure,
                title,
                xAxisTitle
            );
            plotModel.IsLegendVisible = true;
            return plotModel;
        }
    }
}
