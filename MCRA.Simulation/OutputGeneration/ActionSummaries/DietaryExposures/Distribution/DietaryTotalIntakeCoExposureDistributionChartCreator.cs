using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryTotalIntakeCoExposureDistributionChartCreator : ExposureHistogramChartCreatorBase {

        private DietaryTotalIntakeCoExposureDistributionSection _section;
        private string _intakeUnit;

        public DietaryTotalIntakeCoExposureDistributionChartCreator(DietaryTotalIntakeCoExposureDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "e35780fa-dd07-469b-b39f-24b8f9a9df89";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Transformed dietary exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

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
