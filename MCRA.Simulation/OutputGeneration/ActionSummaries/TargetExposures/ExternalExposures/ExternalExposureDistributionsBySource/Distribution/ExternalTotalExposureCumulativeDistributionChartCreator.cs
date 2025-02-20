using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExternalTotalExposureCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private readonly ExternalTotalExposureDistributionSection _section;
        private readonly string _exposureUnit;

        public ExternalTotalExposureCumulativeDistributionChartCreator(ExternalTotalExposureDistributionSection section, string exposureUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _exposureUnit = exposureUnit;
        }
        public override string Title => $"External exposure cumulative exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";
        public override string ChartId {
            get {
                var pictureId = "7c79078b-2eac-4a2b-8f7a-32f2f7f64e2c";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Exposure ({_exposureUnit})"
            );
        }
    }
}
