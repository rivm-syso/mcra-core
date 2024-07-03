using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryUpperIntakeCoExposureDistributionChartCreator : ExposureHistogramChartCreatorBase {

        private DietaryUpperIntakeCoExposureDistributionSection _section;
        private string _intakeUnit;

        public DietaryUpperIntakeCoExposureDistributionChartCreator(DietaryUpperIntakeCoExposureDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "162a235a-6cde-4dae-8d8b-81ec03ec55ec";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Transformed upper dietary exposure distribution ({_section.UpperPercentage:F1}).";

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
