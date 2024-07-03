using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AggregateTotalIntakeDistributionChartCreator : ExposureHistogramChartCreatorBase {

        private AggregateTotalIntakeDistributionSection _section;
        private string _intakeUnit;

        public AggregateTotalIntakeDistributionChartCreator(AggregateTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "b88fe720-50d7-4bd4-a49c-e7bfbe915509";
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
