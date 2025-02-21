using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalAcuteDistributionUpperChartCreator : ExposureHistogramChartCreatorBase {

        private readonly InternalAcuteDistributionUpperSection _section;
        private readonly string _intakeUnit;

        public InternalAcuteDistributionUpperChartCreator(InternalAcuteDistributionUpperSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "9897de17-23f1-4e9b-a1e7-9fa1ceb9d020";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Transformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                _section.IntakeDistributionBinsCoExposure,
                string.Empty,
                _intakeUnit
            );
        }
    }
}
