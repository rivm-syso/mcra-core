using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class StratifiedStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private readonly InternalDistributionTotalSection _section;
        private readonly string _intakeUnit;

        public StratifiedStackedHistogramChartCreator(InternalDistributionTotalSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "2670f024-075c-4746-a3df-4ab06d73efe5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Stratified transformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

        public override PlotModel Create() {
            return create(
                _section.StratifiedIntakeDistributionBins,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}