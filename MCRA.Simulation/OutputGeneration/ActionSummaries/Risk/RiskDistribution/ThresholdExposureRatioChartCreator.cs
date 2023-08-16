using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Annotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ThresholdExposureRatioChartCreator : HistogramChartCreatorBase {

        private ThresholdExposureRatioDistributionSection _section;
        public ThresholdExposureRatioChartCreator(ThresholdExposureRatioDistributionSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "76f40c7b-642c-4282-bdf5-9b256ca670ef";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Risk total distribution ({100 - _section.PercentageZeros:F1}% positives)";

        public override PlotModel Create() {
            return create(_section.RiskDistributionBins, _section.Threshold);
        }

        private PlotModel create(
                List<HistogramBin> binsTransformed,
                double threshold
            ) {
            var bins = binsTransformed.Select(r => new HistogramBin() {
                Frequency = r.Frequency,
                XMinValue = Math.Pow(10, r.XMinValue),
                XMaxValue = Math.Pow(10, r.XMaxValue),
            }).ToList();

            var xtitle = $"Risk ({_section.RiskMetricType.GetShortDisplayName()})";
            var plotModel = createPlotModel(binsTransformed.ToList(), string.Empty, xtitle, OxyColors.Red, OxyColors.DarkRed);

            if (bins.Any() && threshold < bins.Last().XMaxValue) {
                var referenceSeries1 = createReferenceSeries(bins, threshold);
                referenceSeries1.FillColor = OxyColors.LimeGreen;
                referenceSeries1.StrokeColor = OxyColors.ForestGreen;
                plotModel.Series.Add(referenceSeries1);
                var annotation = new LineAnnotation() { X = threshold, Type = LineAnnotationType.Vertical, Color = OxyColors.Black };
                plotModel.Annotations.Add(annotation);
            }
            return plotModel;
        }
    }
}

