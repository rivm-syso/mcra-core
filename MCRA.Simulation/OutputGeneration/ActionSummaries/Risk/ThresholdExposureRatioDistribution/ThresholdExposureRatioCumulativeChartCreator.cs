using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ThresholdExposureRatioCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private ThresholdExposureRatioDistributionSection _section;

        public ThresholdExposureRatioCumulativeChartCreator(ThresholdExposureRatioDistributionSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "3a52c9f9-3b34-4673-b40b-c573e7460d9b";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Risk cumulative total distribution ({100 - _section.PercentageZeros:F1}% positives)";
        public override PlotModel Create() {
            return base.createPlotModel(
                _section.PercentilesGrid,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Threshold value/exposure"
           );
        }
    }
}
