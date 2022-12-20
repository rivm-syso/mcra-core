using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OIMDistributionChartCreator : ExposureHistogramChartCreatorBase {

        private OIMDistributionSection _section;
        private string _intakeUnit;

        public OIMDistributionChartCreator(OIMDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "4ee2644a-3414-4e07-82ae-ec145136f484";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Transformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";
        
        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                null,
                string.Empty,
                _intakeUnit
            );
        }
    }
}
