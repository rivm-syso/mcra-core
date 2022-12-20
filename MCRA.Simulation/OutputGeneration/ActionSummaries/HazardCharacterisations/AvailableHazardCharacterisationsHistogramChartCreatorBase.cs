using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class AvailableHazardCharacterisationsHistogramChartCreatorBase : HistogramChartCreatorBase {

        private readonly string _targetDoseUnit;

        protected readonly ICollection<AvailableHazardCharacterisationsSummaryRecord> _records;

        public AvailableHazardCharacterisationsHistogramChartCreatorBase(
            ICollection<AvailableHazardCharacterisationsSummaryRecord> records,
            string targetDoseUnit,
            int width,
            int height
        ) {
            _records = records;
            _targetDoseUnit = targetDoseUnit;
            Width = width;
            Height = height;
        }

        public override PlotModel Create() {
            return create(_records, _targetDoseUnit);
        }

        public override string Title => $"Histogram of hazard characterisations ({_targetDoseUnit})";

        protected static PlotModel create(
            ICollection<AvailableHazardCharacterisationsSummaryRecord> records,
            string targetDoseUnit
        ) {
            var plotModel = new PlotModel { };
            
            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title =  "Hazard characterisation",
                Unit = targetDoseUnit,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(logarithmicAxis);

            var valueAxis = new LinearAxis {
                Position = AxisPosition.Left,
                Minimum = 0,
                MinimumPadding = 0,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(valueAxis);

            var values = records.Select(r => r.HazardCharacterisation).Where(r => !double.IsNaN(r)).ToList();

            if (values.Any()) {

                var logValues = values.Select(r => Math.Log10(r)).ToList();
                var min = logValues.Min();
                var max = logValues.Max();

                int numberOfBins = Math.Sqrt(values.Count) < 100 ? (int)Math.Ceiling(Math.Sqrt(values.Count)) : 100;
                var bins = logValues
                    .MakeHistogramBins(numberOfBins, min, max)
                    .Select(r => new HistogramBin() {
                        Frequency = r.Frequency,
                        XMinValue = Math.Pow(10, r.XMinValue),
                        XMaxValue = Math.Pow(10, r.XMaxValue),
                    })
                    .ToList();

                logarithmicAxis.Maximum =  bins.GetMaxBound() + bins.AverageBinSize();
                logarithmicAxis.Minimum = bins.GetMinBound();

                var histogramSeries = new HistogramSeries() {
                    FillColor = OxyColors.CornflowerBlue,
                    StrokeColor = OxyColor.FromArgb(255, 78, 132, 233),
                    Items = bins
                };

                plotModel.Series.Add(histogramSeries);
            }

            return plotModel;
        }
    }
}
