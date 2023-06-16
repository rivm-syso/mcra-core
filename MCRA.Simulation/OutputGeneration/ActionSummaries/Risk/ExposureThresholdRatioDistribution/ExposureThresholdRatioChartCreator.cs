using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Annotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureThresholdRatioChartCreator : HistogramChartCreatorBase {

        private ExposureThresholdRatioDistributionSection _section;

        public ExposureThresholdRatioChartCreator(ExposureThresholdRatioDistributionSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "5a1c9ece-57d9-42e8-9a62-ca0db06cf1ff";
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

            var xtitle = $"Exposure/threshold value";
            var plotModel = createPlotModel(binsTransformed.ToList(), string.Empty, xtitle, OxyColors.Red, OxyColors.DarkRed);

            if (bins.Any() && threshold > bins.First().XMaxValue) {
                var referenceSeries1 = createReferenceSeries(bins, threshold, smaller: true);
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

